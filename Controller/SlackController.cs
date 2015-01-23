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
        private Dictionary<Guid, ConcurrentQueue<string>> m_JamQueues; 

        public SlackController(MainForm form)
        {
            _form = form;
            m_JamThreads = new Dictionary<Guid, Thread>();
            m_IsConnected = new ConcurrentDictionary<Guid, SlackConnectionStatus>();
            m_JamQueues = new Dictionary<Guid, ConcurrentQueue<string>>();
        }

        public void RegisterJam(Jam jam)
        {
            var thread = new Thread(Run);
            thread.IsBackground = true;

            m_JamThreads.Add(jam.Guid, thread);
            m_IsConnected.TryAdd(jam.Guid, SlackConnectionStatus.Disconnected);
            m_JamQueues.Add(jam.Guid, new ConcurrentQueue<string>());

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

                                this.SendPong(slack, currentToken);
                            }

                            while (currentToken == jam.ControllerSlackToken)
                            {
                                while (slack.Connected)
                                {
                                    string value;
                                    if (m_JamQueues[jam.Guid].TryDequeue(out value))
                                    {
                                        switch (value)
                                        {
                                            case "pong":
                                                this.SendPong(slack, jam.ControllerSlackToken);
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

                                    this.SendPong(slack, currentToken);
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

        private void SendPong(Slack slack, string token)
        {
            var jamcastChannel =
                slack.Ims.Where(x => x.User == slack.GetUser("jamcast").Id).Select(x => x.Id).FirstOrDefault();
            if (jamcastChannel == null)
            {
                var client = new WebClient();
                var result = client.DownloadString("https://slack.com/api/im.open?token=" + token + "&user=" +
                                                   slack.GetUser("jamcast").Id);
                var response = JsonConvert.DeserializeObject<dynamic>(result);

                jamcastChannel = (string)response.channel.id;
            }

            slack.SendMessage(jamcastChannel, Convert.ToBase64String(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(new
            {
                Source = "",
                Type = "pong",
            }))));
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
            this.m_JamQueues[guid].Enqueue("pong");
        }
    }
}