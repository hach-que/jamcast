using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading;
using GooglePubSub;
using Newtonsoft.Json;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.CSharp.RuntimeBinder;

namespace Controller
{
    public enum PubSubConnectionStatus
    {
        Disconnected,
        Connecting,
        Connected
    }

    public class PubSubController
    {
        private readonly MainForm _form;
        private Dictionary<Guid, Thread> m_JamThreads;
        private ConcurrentDictionary<Guid, PubSubConnectionStatus> m_IsConnected;
        private Dictionary<Guid, ConcurrentQueue<dynamic>> m_JamQueues;
        private Dictionary<Guid, int> _pubSubOperations;
        public PubSub _currentPubSub;

        public PubSubController(MainForm form)
        {
            _form = form;
            m_JamThreads = new Dictionary<Guid, Thread>();
            m_IsConnected = new ConcurrentDictionary<Guid, PubSubConnectionStatus>();
            m_JamQueues = new Dictionary<Guid, ConcurrentQueue<dynamic>>();
            _pubSubOperations = new Dictionary<Guid, int>();
        }

        public int GetPubSubOperations(Jam jam)
        {
            if (_pubSubOperations.ContainsKey(jam.Guid))
            {
                return _pubSubOperations[jam.Guid];
            }

            return 0;
        }

        public void RegisterJam(Jam jam)
        {
            var thread = new Thread(Run);
            thread.IsBackground = true;

            _pubSubOperations.Add(jam.Guid, 0);
            m_JamThreads.Add(jam.Guid, thread);
            m_IsConnected.TryAdd(jam.Guid, PubSubConnectionStatus.Disconnected);
            m_JamQueues.Add(jam.Guid, new ConcurrentQueue<dynamic>());

            thread.Start(jam);
        }
        
        private static OAuthToken GetOAuthTokenFromEndpoint(string currentEndpoint, string controllerSecret)
        {
            var client = new WebClient();
            var jsonResult = client.DownloadString(currentEndpoint + "?controller_secret=" + controllerSecret);
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

        private void Run(object obj)
        {
            var jam = (Jam) obj;

            while (true)
            {
                if (!string.IsNullOrWhiteSpace(jam.GoogleCloudOAuthEndpointURL) && !string.IsNullOrWhiteSpace(jam.GoogleCloudProjectID) 
                    && !string.IsNullOrWhiteSpace(jam.GoogleCloudStorageSecret))
                {
                    Thread.Sleep(100);
                }

                PubSub pubsub = null;
                var currentEndpoint = jam.GoogleCloudOAuthEndpointURL;
                var currentProjectID = jam.GoogleCloudProjectID;
                var currentStorageSecret = jam.GoogleCloudStorageSecret;
                while (currentEndpoint == jam.GoogleCloudOAuthEndpointURL && 
                    currentProjectID == jam.GoogleCloudProjectID &&
                    currentStorageSecret == jam.GoogleCloudStorageSecret)
                {
                    if (pubsub == null)
                    {
                        try
                        {
                            m_IsConnected[jam.Guid] = PubSubConnectionStatus.Connecting;

                            _form.Invoke(new Action(() =>
                            {
                                _form.RefreshConnectionStatus(jam);
                            }));

                            pubsub = new PubSub(currentProjectID, () => GetOAuthTokenFromEndpoint(currentEndpoint, currentStorageSecret));
                            pubsub.CreateTopic("game-jam-bootstrap");
                            pubsub.CreateTopic("game-jam-controller");
                            pubsub.Subscribe("game-jam-controller", "controller");

                            _currentPubSub = pubsub;

                            var pubsubCancellationSource = new CancellationTokenSource();
                            var pubsubCancellationToken = pubsubCancellationSource.Token;
                            var pubsubPollThread = new Thread(() =>
                            {
                                while (!pubsubCancellationToken.IsCancellationRequested)
                                {
                                    _pubSubOperations[jam.Guid] = pubsub.OperationsRequested;

                                    try
                                    {
                                        var messages = pubsub.Poll(1000, false);
                                        foreach (var message in messages)
                                        {
                                            message.Acknowledge();

                                            try
                                            {
                                                var m =
                                                    JsonConvert.DeserializeObject<dynamic>(
                                                        Encoding.ASCII.GetString(Convert.FromBase64String(message.Data)));
                                                var source = (string) m.Source;
                                                _form.Invoke(new Action(() =>
                                                {
                                                    jam.ReceivedClientMessage(this, source, m);
                                                }));
                                            }
                                            catch (Exception ex)
                                            {
                                                Debug.WriteLine(ex);
                                            }
                                        }

                                        if (messages.Count == 0)
                                        {
                                            Thread.Sleep(1000);
                                        }
                                    }
                                    catch (OperationCanceledException ex)
                                    {
                                        throw;
                                    }
                                    catch (Exception ex)
                                    { 
                                        Debug.WriteLine(ex);
                                    }
                                }
                            });
                            
                            try
                            {
                                pubsubPollThread.IsBackground = true;
                                pubsubPollThread.Start();

                                m_IsConnected[jam.Guid] = PubSubConnectionStatus.Connected;

                                _form.Invoke(new Action(() =>
                                {
                                    _form.RefreshConnectionStatus(jam);
                                }));

                                this.SendPong(pubsub, null, null, jam);
                                
                                while (currentEndpoint == jam.GoogleCloudOAuthEndpointURL && currentProjectID == jam.GoogleCloudProjectID &&
                                        currentStorageSecret == jam.GoogleCloudStorageSecret)
                                {
                                    dynamic value;
                                    if (m_JamQueues[jam.Guid].TryDequeue(out value))
                                    {
                                        try
                                        {
                                            if ((string)value.Target != null)
                                            {
                                                var computer = jam.Computers.FirstOrDefault(x => x.Guid.ToString() == (string)value.Target);
                                                if (computer != null)
                                                {
                                                    computer.LastTimeControllerSentMessageToBootstrap = DateTime.UtcNow;
                                                }
                                            }
                                        }
                                        catch (RuntimeBinderException ex)
                                        {
                                            // Ignore this error, it just means there's no target.
                                        }

                                        switch ((string) value.Type)
                                        {
                                            case "pong":
                                                this.SendPong(pubsub, null, null, jam);
                                                break;
                                            case "designate":
                                                this.SendDesignate(pubsub, null, value.Target,
                                                    value.Role);
                                                break;
                                            case "client-settings":
                                                this.SendSettings(pubsub, null, value.Target,
                                                    "client-settings", value.Settings);
                                                break;
                                            case "projector-settings":
                                                this.SendSettings(pubsub, null, value.Target,
                                                    "projector-settings", value.Settings);
                                                break;
                                            case "update-clients":
                                                this.SendUpdateClients(pubsub, value);
                                                break;
                                            default:
                                                this.SendCustom(pubsub, null, value);
                                                break;
                                        }
                                    }
                                }
                            }
                            finally
                            {
                                pubsubCancellationSource.Cancel();
                                pubsub.Unsubscribe("game-jam-controller", "controller");
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex);
                            break;
                        }
                    }
                }

                m_IsConnected[jam.Guid] = PubSubConnectionStatus.Disconnected;

                _form.Invoke(new Action(() =>
                {
                    _form.RefreshConnectionStatus(jam);
                }));

                if (currentEndpoint == jam.GoogleCloudOAuthEndpointURL && currentProjectID == jam.GoogleCloudProjectID &&
                    currentStorageSecret == jam.GoogleCloudOAuthEndpointURL)
                {
                    // Connection lost or unable to connect.
                    Thread.Sleep(5000);
                }
            }
        }

        private void SendUpdateClients(PubSub pubsub, object value)
        {
            pubsub.Publish("projectors", Convert.ToBase64String(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(value))), null);
        }

        internal void SendPong(Guid guid, Computer computer, Jam jam)
        {
            SendPong(_currentPubSub, guid.ToString(), computer, jam);
        }

        private void SendPong(PubSub pubsub, string bootstrapGuid, Computer computer, Jam jam)
        {
            var bootstrapTopic = GetBootstrapTopic(pubsub, bootstrapGuid);

            DateTime sendTime = DateTime.UtcNow;
            DateTime? lastRecieveTime = null;

            if (computer != null)
            {
                computer.LastTimeControllerSentMessageToBootstrap = sendTime;
                lastRecieveTime = computer.LastTimeControllerRecievedMessageFromBootstrap;
            }
            else
            {
                foreach (var c in jam.Computers)
                {
                    c.LastTimeControllerSentMessageToBootstrap = sendTime;
                }
            }

            try {
                pubsub.Publish(bootstrapTopic, Convert.ToBase64String(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(new
                {
                    Target = bootstrapGuid ?? "",
                    Type = "pong",
                    AvailableClientVersion = jam.AvailableClientVersion,
                    AvailableProjectorVersion = jam.AvailableProjectorVersion,
                    AvailableBootstrapVersion = jam.AvailableBootstrapVersion,
                    AvailableClientFile = jam.AvailableClientFile,
                    AvailableProjectorFile = jam.AvailableProjectorFile,
                    AvailableBootstrapFile = jam.AvailableBootstrapFile,
                    SendTime = sendTime,
                    LastRecieveTime = lastRecieveTime,
                }))), null);
            }
            catch (WebException ex)
            {
                var httpWebResponse = ex.Response as HttpWebResponse;
                if (httpWebResponse != null)
                {
                    if (httpWebResponse.StatusCode == HttpStatusCode.NotFound)
                    {
                        // Try to send to the global bootstrap topic instead.
                        pubsub.Publish(GetBootstrapTopic(pubsub, null),
                            Convert.ToBase64String(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(new
                            {
                                Target = bootstrapGuid ?? "",
                                Type = "pong",
                                AvailableClientVersion = jam.AvailableClientVersion,
                                AvailableProjectorVersion = jam.AvailableProjectorVersion,
                                AvailableBootstrapVersion = jam.AvailableBootstrapVersion,
                                AvailableClientFile = jam.AvailableClientFile,
                                AvailableProjectorFile = jam.AvailableProjectorFile,
                                AvailableBootstrapFile = jam.AvailableBootstrapFile,
                                SendTime = sendTime,
                                LastRecieveTime = lastRecieveTime,
                            }))), null);
                    }
                    else
                    {
                        throw;
                    }
                }
                else
                {
                    throw;
                }
            }
        }

        private void SendDesignate(PubSub pubsub, string bootstrapGuid, string target, string role)
        {
            var bootstrapTopic = GetBootstrapTopic(pubsub, bootstrapGuid);
            
            pubsub.Publish(bootstrapTopic, Convert.ToBase64String(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(new
            {
                Target = target,
                Type = "designate",
                Role = role,
            }))), null);
        }

        private void SendSettings(PubSub pubsub, string bootstrapGuid, string target, string settingsType, string settings)
        {
            var bootstrapTopic = GetBootstrapTopic(pubsub, bootstrapGuid);

            pubsub.Publish(bootstrapTopic, Convert.ToBase64String(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(new
            {
                Target = target,
                Type = settingsType,
                Settings = settings,
            }))), null);
        }

        private void SendCustom(PubSub pubsub, string bootstrapGuid, object data)
        {
            var bootstrapTopic = GetBootstrapTopic(pubsub, bootstrapGuid);

            pubsub.Publish(
                bootstrapTopic,
                Convert.ToBase64String(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(data))), 
                null);
        }

        private static string GetBootstrapTopic(PubSub pubsub, string bootstrapGuid)
        {
            if (bootstrapGuid == null)
            {
                return "game-jam-bootstrap";
            }
            else
            {
                return "bootstrap-" + bootstrapGuid;
            }
        }

        public PubSubConnectionStatus GetConnectionStatus(Guid guid)
        {
            PubSubConnectionStatus value;
            if (this.m_IsConnected.TryGetValue(guid, out value))
            {
                return value;
            }

            return PubSubConnectionStatus.Disconnected;
        }

        public void ScanComputers(Guid guid)
        {
            this.m_JamQueues[guid].Enqueue(new { Type = "pong" });
        }

        public void DesignateComputer(Guid jamGuid, Guid computerGuid, Role role)
        {
            this.m_JamQueues[jamGuid].Enqueue(new { Type = "designate", Target = computerGuid.ToString(), Role = role.ToString() });
        }

        public void UpdateComputerSettings(Guid jamGuid, Guid computerGuid, string settingsType, string settingsJson)
        {
            this.m_JamQueues[jamGuid].Enqueue(new { Type = settingsType, Target = computerGuid.ToString(), Settings = settingsJson });
        }

        public void SendCustomMessage(Guid guid, object data)
        {
            this.m_JamQueues[guid].Enqueue(data);
        }
    }
}