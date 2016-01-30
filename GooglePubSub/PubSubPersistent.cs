using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace GooglePubSub
{
    public class PubSubPersistent
    {
        private string _pubSubOAuthEndpointURL;

        private string _pubSubProjectID;

        private Thread _thread;

        private PubSub _pubSubInstance;

        private bool _running = true;

        public PubSubPersistentStatus Status { get; private set; }

        public Action<string, Exception> LogException { get; set; }

        public List<string> TopicsToCreate { get; private set; } 

        public List<KeyValuePair<string, string>> SubscriptionsToCreate { get; private set; }

        public event MessageReceivedEventHandler MessageRecieved;

        private List<KeyValuePair<string, dynamic>> _queuedMessages; 

        public PubSubPersistent()
        {
            Status = PubSubPersistentStatus.Disconnected;
            LogException = (t, ex) => Debug.WriteLine("Exception at '" + t + "': " + ex);
            TopicsToCreate = new List<string>();
            SubscriptionsToCreate = new List<KeyValuePair<string, string>>();
            this._queuedMessages = new List<KeyValuePair<string, dynamic>>();
            Timeout = 120;

            this._thread = new Thread(Run);
            this._thread.IsBackground = true;
            this._thread.Start();
        }

        public int Timeout { get; set; }

        public void Reconfigure(
            string oAuthEndpointURL,
            string projectID)
        {
            this._pubSubOAuthEndpointURL = oAuthEndpointURL;
            this._pubSubProjectID = projectID;
        }

        public void Stop()
        {
            _running = false;
        }

        public void Publish(string topic, dynamic message)
        {
            if (this._pubSubInstance != null)
            {
                this._pubSubInstance.Publish(topic,
                    Convert.ToBase64String(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(message))), null);
            }
            else
            {
                _queuedMessages.Add(new KeyValuePair<string, dynamic>(topic, message));
            }
        }

        private void Run()
        {
            try
            {
                while (_running)
                {
                    try
                    {
                        Status = PubSubPersistentStatus.Connecting;

                        _pubSubInstance = new PubSub(this._pubSubProjectID, this.GetOAuthTokenFromEndpoint);
                        _pubSubInstance.Timeout = Timeout;

                        var topics = new List<string>(TopicsToCreate);
                        var subscriptions = new List<KeyValuePair<string, string>>(SubscriptionsToCreate);

                        foreach (var topic in topics)
                        {
                            _pubSubInstance.CreateTopic(topic);
                        }

                        foreach (var subscription in subscriptions)
                        {
                            _pubSubInstance.Subscribe(subscription.Key, subscription.Value);
                        }

                        var queued = new List<KeyValuePair<string, dynamic>>(this._queuedMessages);
                        foreach (var q in queued)
                        {
                            this._pubSubInstance.Publish(q.Key,
                                Convert.ToBase64String(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(q.Value))), null);
                        }
                        this._queuedMessages.RemoveAll(x => queued.Contains(x));

                        try
                        {
                            while (_running)
                            {
                                try
                                {
                                    Status = PubSubPersistentStatus.Connected;

                                    var messages = _pubSubInstance.Poll(100, false);
                                    foreach (var message in messages)
                                    {
                                        message.Acknowledge();

                                        try
                                        {
                                            var m =
                                                JsonConvert.DeserializeObject<dynamic>(
                                                    Encoding.ASCII.GetString(Convert.FromBase64String(message.Data)));

                                            if (this.MessageRecieved != null)
                                            {
                                                this.MessageRecieved(
                                                    this,
                                                    new MessageReceivedEventHandlerArgs { Message = m });
                                            }
                                        }
                                        catch (Exception messageEx)
                                        {
                                            LogException("Message", messageEx);
                                        }
                                    }
                                }
                                catch (Exception innerEx)
                                {
                                    Status = PubSubPersistentStatus.Connecting;

                                    LogException("Inner", innerEx);
                                }
                            }
                        }
                        catch (Exception outerEx)
                        {
                            LogException("Outer", outerEx);
                        }
                        finally
                        {
                            foreach (var subscription in subscriptions)
                            {
                                _pubSubInstance.Unsubscribe(subscription.Key, subscription.Value);
                            }
                        }
                    }
                    catch (Exception setupEx)
                    {
                        LogException("Setup", setupEx);
                    }

                    Status = PubSubPersistentStatus.Disconnected;
                }
            }
            catch (Exception finalEx)
            {
                LogException("Final", finalEx);
            }
            finally
            {
                Status = PubSubPersistentStatus.Disconnected;
            }
        }

        #region Token Retrieval
        
        private OAuthToken GetOAuthTokenFromEndpoint()
        {
            while (true)
            {
                try
                {
                    var client = new WebClient();
                    var jsonResult = client.DownloadString(_pubSubOAuthEndpointURL);
                    var json = JsonConvert.DeserializeObject<dynamic>(jsonResult);
                    if (!(bool)json.has_error)
                    {
                        var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                        dtDateTime = dtDateTime.AddSeconds((double)(json.result.created + json.result.expires_in));
                        return new OAuthToken
                        {
                            ExpiryUtc = dtDateTime,
                            AccessToken = json.result.access_token
                        };
                    }

                    throw new Exception("Error when retrieving access token: " + json.error);
                }
                catch (WebException ex)
                {
                    if (ex.Status == WebExceptionStatus.Timeout)
                    {
                        continue;
                    }

                    var response = ex.Response as HttpWebResponse;
                    if (response != null)
                    {
                        if (response.StatusCode == HttpStatusCode.RequestTimeout ||
                            response.StatusCode == HttpStatusCode.GatewayTimeout)
                        {
                            continue;
                        }
                    }

                    throw;
                }
            }
        }

        #endregion
    }
}
