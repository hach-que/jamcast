using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Linq;
using Newtonsoft.Json;
using SlackRTM;

namespace Controller
{
    public enum SlackConnectionStatus
    {
        Disconnected,
        Connecting,
        Connected
    }

    public class SlackController
    {
        private readonly MainForm _form;
        private Dictionary<Guid, Thread> m_JamThreads;
        private ConcurrentDictionary<Guid, SlackConnectionStatus> m_IsConnected;
        private Dictionary<Guid, ConcurrentQueue<dynamic>> m_JamQueues; 

        public SlackController(MainForm form)
        {
            _form = form;
            m_JamThreads = new Dictionary<Guid, Thread>();
            m_IsConnected = new ConcurrentDictionary<Guid, SlackConnectionStatus>();
            m_JamQueues = new Dictionary<Guid, ConcurrentQueue<dynamic>>();
        }

        public void RegisterJam(Jam jam)
        {
            var thread = new Thread(Run);
            thread.IsBackground = true;

            m_JamThreads.Add(jam.Guid, thread);
            m_IsConnected.TryAdd(jam.Guid, SlackConnectionStatus.Disconnected);
            m_JamQueues.Add(jam.Guid, new ConcurrentQueue<dynamic>());

            thread.Start(jam);
        }

        private void Run(object obj)
        {
            var jam = (Jam) obj;

            while (true)
            {
                if (!string.IsNullOrWhiteSpace(jam.ControllerSlackToken))
                {
                    Thread.Sleep(100);
                }

                Slack slack = null;
                var currentToken = jam.ControllerSlackToken;
                while (currentToken == jam.ControllerSlackToken)
                {
                    if (slack == null)
                    {
                        try
                        {
                            m_IsConnected[jam.Guid] = SlackConnectionStatus.Connecting;

                            _form.Invoke(new Action(() =>
                            {
                                _form.RefreshConnectionStatus(jam);
                            }));

                            slack = new Slack();
                            slack.Init(currentToken);
                            var slackCopy = slack;
                            slack.OnEvent += (sender, e) =>
                            {
                                var message = e.Data as SlackRTM.Events.Message;
                                if (message == null)
                                {
                                    return;
                                }

                                if (message.Hidden)
                                {
                                    return;
                                }
                                
                                if (message.Channel[0] == 'D') // DMs.
                                {
                                    var userId = message.User;
                                    var user = slackCopy.GetUser(userId).Name;

                                    if (user == "jamcast")
                                    {
                                        try
                                        {
                                            var m =
                                                JsonConvert.DeserializeObject<dynamic>(
                                                    Encoding.ASCII.GetString(Convert.FromBase64String(message.Text)));
                                            var source = (string)m.Source;
                                            _form.Invoke(new Action(() =>
                                            {
                                                jam.ReceivedClientMessage(source, m);
                                            }));
                                        }
                                        catch (FormatException)
                                        {
                                            // Badly formatted message; ignore it.
                                        }
                                    }
                                }
                            };

                            slack.Connect();

                            if (slack.Connected)
                            {
                                m_IsConnected[jam.Guid] = SlackConnectionStatus.Connected;

                                _form.Invoke(new Action(() =>
                                {
                                    _form.RefreshConnectionStatus(jam);
                                }));

                                this.SendPong(slack, currentToken, jam);
                            }

                            while (currentToken == jam.ControllerSlackToken)
                            {
                                while (slack.Connected)
                                {
                                    dynamic value;
                                    if (m_JamQueues[jam.Guid].TryDequeue(out value))
                                    {
                                        switch ((string)value.Type)
                                        {
                                            case "pong":
                                                this.SendPong(slack, jam.ControllerSlackToken, jam);
                                                break;
                                            case "designate":
                                                this.SendDesignate(slack, jam.ControllerSlackToken, value.Target, value.Role);
                                                break;
                                            case "client-settings":
                                                this.SendSettings(slack, jam.ControllerSlackToken, value.Target, "client-settings", value.Settings);
                                                break;
                                            case "projector-settings":
                                                this.SendSettings(slack, jam.ControllerSlackToken, value.Target, "projector-settings", value.Settings);
                                                break;
                                            default:
                                                this.SendCustom(slack, jam.ControllerSlackToken, value);
                                                break;
                                        }
                                    }

                                    Thread.Sleep(100);
                                }

                                m_IsConnected[jam.Guid] = SlackConnectionStatus.Connecting;

                                _form.Invoke(new Action(() =>
                                {
                                    _form.RefreshConnectionStatus(jam);
                                }));

                                slack.Init(currentToken);
                                slack.Connect();

                                if (slack.Connected)
                                {
                                    m_IsConnected[jam.Guid] = SlackConnectionStatus.Connected;

                                    _form.Invoke(new Action(() =>
                                    {
                                        _form.RefreshConnectionStatus(jam);
                                    }));

                                    this.SendPong(slack, currentToken, jam);
                                }
                            }
                        }
                        catch (Exception)
                        {
                            break;
                        }
                    }
                }

                m_IsConnected[jam.Guid] = SlackConnectionStatus.Disconnected;

                _form.Invoke(new Action(() =>
                {
                    _form.RefreshConnectionStatus(jam);
                }));

                if (currentToken == jam.ControllerSlackToken)
                {
                    // Connection lost or unable to connect.
                    Thread.Sleep(5000);
                }
            }
        }

        private void SendPong(Slack slack, string token, Jam jam)
        {
            var jamcastChannel = GetJamcastChannel(slack, token);

            slack.SendMessage(jamcastChannel, Convert.ToBase64String(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(new
            {
                Target = "",
                Type = "pong",
                AvailableClientVersion = jam.AvailableClientVersion,
                AvailableProjectorVersion = jam.AvailableProjectorVersion,
                AvailableClientFile = jam.AvailableClientFile,
                AvailableProjectorFile = jam.AvailableProjectorFile,
            }))));
        }

        private void SendDesignate(Slack slack, string token, string target, string role)
        {
            var jamcastChannel = GetJamcastChannel(slack, token);

            slack.SendMessage(jamcastChannel, Convert.ToBase64String(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(new
            {
                Target = target,
                Type = "designate",
                Role = role,
            }))));
        }

        private void SendSettings(Slack slack, string token, string target, string settingsType, string settings)
        {
            var jamcastChannel = GetJamcastChannel(slack, token);

            slack.SendMessage(jamcastChannel, Convert.ToBase64String(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(new
            {
                Target = target,
                Type = settingsType,
                Settings = settings,
            }))));
        }

        private void SendCustom(Slack slack, string token, object data)
        {
            var jamcastChannel = GetJamcastChannel(slack, token);

            slack.SendMessage(jamcastChannel, Convert.ToBase64String(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(data))));
        }

        private static string GetJamcastChannel(Slack slack, string token)
        {
            var jamcastChannel =
                slack.Ims.Where(x => x.User == slack.GetUser("jamcast").Id).Select(x => x.Id).FirstOrDefault();
            if (jamcastChannel == null)
            {
                var client = new WebClient();
                var result = client.DownloadString("https://slack.com/api/im.open?token=" + token + "&user=" +
                                                   slack.GetUser("jamcast").Id);
                var response = JsonConvert.DeserializeObject<dynamic>(result);

                jamcastChannel = (string) response.channel.id;
            }
            return jamcastChannel;
        }

        public SlackConnectionStatus GetConnectionStatus(Guid guid)
        {
            SlackConnectionStatus value;
            if (this.m_IsConnected.TryGetValue(guid, out value))
            {
                return value;
            }

            return SlackConnectionStatus.Disconnected;
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