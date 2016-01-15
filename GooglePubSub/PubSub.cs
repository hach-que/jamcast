using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GooglePubSub
{
    public class PubSub
    {
        private readonly string _projectName;
        private readonly List<string> _subscriptions;

        private Func<OAuthToken> _getToken; 
        private OAuthToken _token;

        /// <summary>
        /// Creates a new node in a many-to-many PubSub system.
        /// </summary>
        /// <param name="projectName">
        /// A project URL like "melbourne-global-game-jam-16"
        /// </param>
        /// <param name="getBearerToken">
        /// The API bearer token to use.
        /// </param>
        public PubSub(string projectName, Func<OAuthToken> getBearerToken)
        {
            _projectName = projectName;
            _subscriptions = new List<string>();
            _getToken = getBearerToken;

            CheckToken();
        }

        private WebClient MakeClient()
        {
            var client = new WebClient();
            client.Headers["Authorization"] = "Bearer " + _token.AccessToken;
            return client;
        }

        private void CheckToken()
        {
            if (_token == null)
            {
                _token = _getToken();
            }
            if (_token.ExpiryUtc < DateTime.UtcNow)
            {
                _token = _getToken();
            }
        }

        public void CreateTopic(string topic)
        {
            CheckToken();

            var request = new
            {
                name = "projects/" + _projectName + "/topics/" + topic
            };
            var requestSerialized = JsonConvert.SerializeObject(request);

            try
            {
                MakeRetryableRequest(
                    "https://pubsub.googleapis.com/v1/" + request.name,
                    "PUT",
                    requestSerialized);
            }
            catch (WebException ex)
            {
                var webResponse = ex.Response as HttpWebResponse;
                if (webResponse != null && webResponse.StatusCode == HttpStatusCode.Conflict)
                {
                    // This is fine, the topic already exists.
                    return;
                }

                throw;
            }
        }

        public void Subscribe(string topic, string subscription)
        {
            CheckToken();

            var request = new
            {
                name = "projects/" + _projectName + "/subscriptions/t-" + topic + "-s-" + subscription,
                topic = "projects/" + _projectName + "/topics/" + topic,
                ackDeadlineSeconds = 0,
            };
            var requestSerialized = JsonConvert.SerializeObject(request);
            try
            {
                MakeRetryableRequest(
                    "https://pubsub.googleapis.com/v1/projects/" + _projectName + "/subscriptions/t-" + topic + "-s-" +
                    subscription,
                    "PUT",
                    requestSerialized);
            }
            catch (WebException ex)
            {
                var webResponse = ex.Response as HttpWebResponse;
                if (webResponse != null && webResponse.StatusCode == HttpStatusCode.Conflict)
                {
                    // This is fine, the subscription already exists.
                }
                else
                {
                    throw;
                }
            }
            if (!_subscriptions.Contains("t-" + topic + "-s-" + subscription))
            {
                _subscriptions.Add("t-" + topic + "-s-" + subscription);
            }
        }

        public List<Message> Poll(int messagesPerSubscription, bool immediate)
        {
            CheckToken();

            Debug.WriteLine("POLL START");

            var messages = new ConcurrentBag<Message>();

            if (_subscriptions.Count == 0)
            {
                throw new InvalidOperationException("You can't poll when you're not subscribed to anything!");
            }

            var webClients = new ConcurrentBag<WebClient>();
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            var tasks = (from topicSubscription in _subscriptions
                select Task.Run(async () =>
                {
                    var request = new
                    {
                        maxMessages = messagesPerSubscription,
                        returnImmediately = immediate,
                    };
                    var requestSerialized = JsonConvert.SerializeObject(request);
                    try
                    {
                        Debug.WriteLine("POLL TASK STARTED OPERATION");

                        string responseSerialized;
                        try
                        {
                            var client = MakeClient();
                            webClients.Add(client);
                            responseSerialized = await client.UploadStringTaskAsync(
                                "https://pubsub.googleapis.com/v1/projects/" + _projectName + "/subscriptions/" +
                                topicSubscription + ":pull",
                                "POST",
                                requestSerialized);
                        }
                        catch (WebException ex)
                        {
                            if (ex.Status == WebExceptionStatus.KeepAliveFailure)
                            {
                                // This is expected for long polling.
                                responseSerialized = "{}";
                            }
                            else
                            {
                                throw;
                            }
                        }

                        Debug.WriteLine("POLL TASK RESPONSE RECEIVED");

                        var response = JsonConvert.DeserializeObject<dynamic>(responseSerialized);
                        if (response.receivedMessages != null)
                        {
                            foreach (var msg in response.receivedMessages)
                            {
                                messages.Add(new Message(MakeClient(),
                                    "https://pubsub.googleapis.com/v1/projects/" + _projectName + "/subscriptions/" +
                                    topicSubscription + ":acknowledge", (string)msg.ackId)
                                {
                                    Data = msg.message.data,
                                    Attributes = msg.message.attributes,
                                });
                            }
                        }
                    }
                    finally
                    {
                        Debug.WriteLine("POLL TASK FINISHED OPERATION");
                    }
                }, cancellationToken)).ToArray();

            if (immediate)
            {
                Task.WaitAll(tasks);
            }
            else
            {
                Task.WaitAny(tasks);
                foreach (var client in webClients)
                {
                    client.CancelAsync();
                }
                cancellationTokenSource.Cancel();
                try
                {
                    Task.WaitAll(tasks);
                }
                catch (Exception)
                {
                    // ignored
                }
            }
            
            Debug.WriteLine("POLL FINISHED");

            if (immediate && tasks.Any(x => x.Status == TaskStatus.Faulted))
            {
                throw new AggregateException(tasks.Where(x => x.Exception != null).SelectMany(x => x.Exception.InnerExceptions));
            }

            if (!immediate && tasks.Count(x => x.Status == TaskStatus.Faulted) == tasks.Length)
            {
                throw new AggregateException(tasks.Where(x => x.Exception != null).SelectMany(x => x.Exception.InnerExceptions));
            }

            return messages.ToList();
        }

        public void Publish(string topic, string data, Dictionary<string, string> attributes)
        {
            CheckToken();

            var message = new Dictionary<string, object>();
            if (data != null)
            {
                message["data"] = data;
            }
            if (attributes != null)
            {
                message["attributes"] = attributes;
            }
            if (data == null && attributes == null)
            {
                throw new InvalidOperationException("You must specify data or attributes (or both).");
            }

            var request = new
            {
                messages = new[]
                {
                    message
                }
            };
            var requestSerialized = JsonConvert.SerializeObject(request);
            MakeRetryableRequest(
                "https://pubsub.googleapis.com/v1/projects/" + _projectName + "/topics/" + topic + ":publish",
                "POST",
                requestSerialized);
        }

        private void MakeRetryableRequest(string url, string method, string data)
        {
            var succeeded = false;
            while (!succeeded)
            {
                try
                {
                    CheckToken();
                    MakeClient().UploadString(url, method, data);
                    succeeded = true;
                }
                catch (WebException ex)
                {
                    var httpWebResponse = ex.Response as HttpWebResponse;
                    if (httpWebResponse != null)
                    {
                        if (httpWebResponse.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            // Retry with CheckToken.
                            Thread.Sleep(1);
                            continue;
                        }
                    }
                    if (ex.Status == WebExceptionStatus.Timeout)
                    {
                        Thread.Sleep(1);
                        continue;
                    }
                    throw;
                }
            }
        }

        public void Unsubscribe(string topic, string subscription)
        {
            CheckToken();
            
            MakeRetryableRequest(
                "https://pubsub.googleapis.com/v1/projects/" + _projectName + "/subscriptions/" + subscription,
                "DELETE",
                string.Empty);
        }

        public void DeleteTopic(string topic)
        {
            CheckToken();

            MakeRetryableRequest(
                "https://pubsub.googleapis.com/v1/projects/" + _projectName + "/topics/" + topic,
                "DELETE",
                string.Empty);
        }
    }
}