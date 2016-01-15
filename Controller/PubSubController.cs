﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Linq;
using GooglePubSub;
using Newtonsoft.Json;

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

        public PubSubController(MainForm form)
        {
            _form = form;
            m_JamThreads = new Dictionary<Guid, Thread>();
            m_IsConnected = new ConcurrentDictionary<Guid, PubSubConnectionStatus>();
            m_JamQueues = new Dictionary<Guid, ConcurrentQueue<dynamic>>();
        }

        public void RegisterJam(Jam jam)
        {
            var thread = new Thread(Run);
            thread.IsBackground = true;

            m_JamThreads.Add(jam.Guid, thread);
            m_IsConnected.TryAdd(jam.Guid, PubSubConnectionStatus.Disconnected);
            m_JamQueues.Add(jam.Guid, new ConcurrentQueue<dynamic>());

            thread.Start(jam);
        }
        
        private static OAuthToken GetOAuthTokenFromEndpoint(string endpoint)
        {
            var client = new WebClient();
            var jsonResult = client.DownloadString(endpoint);
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
                if (!string.IsNullOrWhiteSpace(jam.GoogleCloudOAuthEndpointURL) && !string.IsNullOrWhiteSpace(jam.GoogleCloudProjectID))
                {
                    Thread.Sleep(100);
                }

                PubSub pubsub = null;
                var currentEndpoint = jam.GoogleCloudOAuthEndpointURL;
                var currentProjectID = jam.GoogleCloudProjectID;
                while (currentEndpoint == jam.GoogleCloudOAuthEndpointURL && currentProjectID == jam.GoogleCloudProjectID)
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

                            pubsub = new PubSub(currentProjectID, () => GetOAuthTokenFromEndpoint(currentEndpoint));
                            pubsub.CreateTopic("game-jam-bootstrap");
                            pubsub.CreateTopic("game-jam-controller");
                            pubsub.Subscribe("game-jam-controller", "controller");

                            var pubsubCancellationSource = new CancellationTokenSource();
                            var pubsubCancellationToken = pubsubCancellationSource.Token;
                            var pubsubPollThread = new Thread(() =>
                            {
                                while (!pubsubCancellationToken.IsCancellationRequested)
                                {
                                    try
                                    {
                                        var message = pubsub.Poll(1, false).FirstOrDefault();
                                        if (message != null)
                                        {
                                            message.Acknowledge();
                                            
                                            var m =
                                                JsonConvert.DeserializeObject<dynamic>(
                                                    Encoding.ASCII.GetString(Convert.FromBase64String(message.Data)));
                                            var source = (string)m.Source;
                                            _form.Invoke(new Action(() =>
                                            {
                                                jam.ReceivedClientMessage(source, m);
                                            }));
                                        }

                                        Thread.Sleep(1);
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

                                this.SendPong(pubsub, null, jam);
                                
                                while (currentEndpoint == jam.GoogleCloudOAuthEndpointURL && currentProjectID == jam.GoogleCloudProjectID)
                                {
                                    dynamic value;
                                    if (m_JamQueues[jam.Guid].TryDequeue(out value))
                                    {
                                        switch ((string) value.Type)
                                        {
                                            case "pong":
                                                this.SendPong(pubsub, null, jam);
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
                        catch (Exception)
                        {
                            break;
                        }
                    }
                }

                m_IsConnected[jam.Guid] = PubSubConnectionStatus.Disconnected;

                _form.Invoke(new Action(() =>
                {
                    _form.RefreshConnectionStatus(jam);
                }));

                if (currentEndpoint == jam.GoogleCloudOAuthEndpointURL && currentProjectID == jam.GoogleCloudProjectID)
                {
                    // Connection lost or unable to connect.
                    Thread.Sleep(5000);
                }
            }
        }

        private void SendPong(PubSub pubsub, string bootstrapGuid, Jam jam)
        {
            var bootstrapTopic = GetBootstrapTopic(pubsub, bootstrapGuid);

            pubsub.Publish(bootstrapTopic, Convert.ToBase64String(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(new
            {
                Target = "",
                Type = "pong",
                AvailableClientVersion = jam.AvailableClientVersion,
                AvailableProjectorVersion = jam.AvailableProjectorVersion,
                AvailableClientFile = jam.AvailableClientFile,
                AvailableProjectorFile = jam.AvailableProjectorFile,
            }))), null);
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