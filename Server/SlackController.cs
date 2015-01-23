using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Compat.Web;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Dapr.WebSockets;
using Newtonsoft.Json;
using SlackRTM;
//using SlackAPI;

namespace JamCast
{
    class SlackController
    {
        private readonly Manager m_Manager;
        private Thread m_SlackThread;
        //private IObservableSocket m_Socket;
        private ConcurrentQueue<string> m_MessagesQueued;
        //private IDisposable m_Subscription;
        private List<string> m_Messages;
        private Slack p_Slack;

        public SlackController(Manager manager)
        {
            m_Manager = manager;
            this.m_Messages = new List<string>();
            this.m_MessagesQueued = new ConcurrentQueue<string>();

            this.m_SlackThread = new Thread(this.Run);
            this.m_SlackThread.IsBackground = true;
            this.m_SlackThread.Start();
        }

        private void Run()
        {
            //var client = new WebClient();
            //var result = client.DownloadString("https://slack.com/api/rtm.start?token=" + AppSettings.SlackToken);
            //var data = JsonConvert.DeserializeObject<dynamic>(result);

            //var channelMap = new Dictionary<string, string>();
            //foreach (var v in data.channels)
            //{
            //    channelMap[(string)v.id] = (string)v.name;
            //}

            //var userMap = new Dictionary<string, string>();
            //foreach (var v in data.users)
            //{
            //    userMap[(string)v.id] = (string)v.name;
            //}

            //var imsMap = new Dictionary<string, string>();
            //foreach (var v in data.ims)
            //{
            //    imsMap[(string)v.id] = (string)v.user;
            //}

            //bool running = true;
            //this.m_Socket = WebSocket.Connect(new Uri((string)data.url), new CancellationToken());
            //this.m_Subscription = this.m_Socket.Incoming.Subscribe(
            //    x =>
            //    {
            //        var message = JsonConvert.DeserializeObject<dynamic>(x);

            //        switch ((string)message.type)
            //        {
            //            case "message":
            //                // get channel and user name.
            //                if (channelMap.ContainsKey((string) message.channel))
            //                {
            //                    if (AppSettings.SlackChannels.Contains(channelMap[(string) message.channel]))
            //                    {
            //                        var user = userMap[(string) message.user];
            //                        this.m_MessagesQueued.Enqueue(user + ": " + (string) message.text);
            //                    }
            //                }
            //                else if (imsMap.ContainsKey((string) message.channel))
            //                {
            //                    var userId = imsMap[(string) message.channel];
            //                    var user = userMap[userId];
            //                    this.HandleCommand(userId, user, channelMap.First(kv => kv.Value == AppSettings.SlackChannels.First()).Key, (string) message.channel, (string)message.text);
            //                }

            //                break;
            //        }
            //    },
            //    ex =>
            //    {
            //        this.m_MessagesQueued.Enqueue(ex.ToString());
            //    },
            //    () =>
            //    {
            //        this.m_MessagesQueued.Enqueue("WEBSOCKET DISCONNECTED!");
            //        running = false;
            //    });

            p_Slack = new Slack();
            p_Slack.Init(AppSettings.SlackToken);
            p_Slack.OnEvent += (s, e) =>
            {
                if (e.Data.Type == "message")
                {
                    var message = e.Data as SlackRTM.Events.Message;
                    if (message.Hidden) // Slack sometimes sends Hidden messages. [Edits, deletes, and other updates.]
                        return;         // They're often unreliable and not useful for bots.  Let's just drop them.
                    if (message.Channel[0] == 'C') // Channels
                    {
                        if (AppSettings.SlackChannels.Contains(p_Slack.GetChannel(message.Channel).Name))
                        {
                            var user = p_Slack.GetUser(message.User).Name;
                            this.m_MessagesQueued.Enqueue(user + ": " + message.Text);
                        }
                    }
                    else if (message.Channel[0] == 'D') // DMs.
                    {
                        var userId = message.User;
                        var user = p_Slack.GetUser(userId).Name;
                        this.HandleCommand(userId, user, p_Slack.GetChannel(AppSettings.SlackChannels.First()).Id, message.Channel, message.Text);
                    }

                }
            };
            p_Slack.Connect();

            while (true)
            {
                while (p_Slack.Connected)
                {
                    Thread.Sleep(100);
                }
                p_Slack.Init(AppSettings.SlackToken);
                p_Slack.Connect();
            }

        }

        private void HandleCommand(string userId, string user, string primaryChannelId, string imId, string text)
        {
            var args = text.Split(' ');
            var cmd = "";
            if (args.Length >= 1)
            {
                cmd = args[0];
            }

            switch (cmd)
            {
                case "share":
                    switch (args.Length)
                    {
                        case 1:
                            // share with no arguments
                            if (this.m_Manager.SetSpecificClient(user))
                            {
                                this.RespondPrimary(primaryChannelId,
                                    "Showing @" + user + "'s screen on all projectors at their request.");
                            }
                            else
                            {
                                this.RespondPrimary(imId, 
                                    "There are no JamCast clients associated with your Slack account name.  Make sure you entered \"" + 
                                    user + "\" (without quotes) when starting the JamCast client.");
                            }

                            break;
                        case 2:
                            // share with user argument
                            if (this.m_Manager.SetSpecificClient(args[1]))
                            {
                                this.RespondPrimary(primaryChannelId,
                                    "Showing @" + args[1] + "'s screen on all projectors at @" + user + "'s request.");
                            }
                            else
                            {
                                this.RespondPrimary(imId,
                                    "There are no JamCast clients associated with the account name @" + args[1]);
                            }

                            break;
                        case 3:
                            // share with user and projector argument.
                            if (AppSettings.ProjectorName == args[2])
                            {
                                if (this.m_Manager.SetSpecificClient(args[1]))
                                {
                                    this.Respond(primaryChannelId,
                                        "Showing @" + args[1] + "'s screen on " + AppSettings.ProjectorName + " projector at @" + user +
                                        "'s request.");
                                }
                                else
                                {
                                    this.Respond(imId,
                                        "There are no JamCast clients associated with the account name @" + args[1] + " on the " + AppSettings.ProjectorName + " projector");
                                }
                            }
                            break;
                    }
                    break;
                case "projectors":
                    var lockingInfo = "";
                    if (this.m_Manager.IsLocked)
                    {
                        lockingInfo = "locked to " + this.m_Manager.LockedClientName + "'s screen by @" +
                                      this.m_Manager.LockingUserName;
                    }

                    if (AppSettings.IsPrimary)
                    {
                        if (this.m_Manager.IsLocked)
                        {
                            this.Respond(imId, AppSettings.ProjectorName + " (primary projector, " + lockingInfo + ")");
                        }
                        else
                        {
                            this.Respond(imId, AppSettings.ProjectorName + " (primary projector)");
                        }
                    }
                    else
                    {
                        if (this.m_Manager.IsLocked)
                        {
                            this.Respond(imId, AppSettings.ProjectorName + " (" + lockingInfo + ")");
                        }
                        else
                        {
                            this.Respond(imId, AppSettings.ProjectorName);
                        }
                    }
                    break;
                case "users":
                    if (args.Length >= 2)
                    {
                        if (AppSettings.ProjectorName == args[1])
                        {
                            this.Respond(imId, string.Join("\r\n", this.m_Manager.GetClientNames().Select(x => "@" + x)));
                        }
                    }
                    else
                    {
                        this.RespondPrimary(imId, string.Join("\r\n", this.m_Manager.GetClientNames().Select(x => "@" + x)));
                    }
                    break;
                case "lock":
                    if (args.Length < 3)
                    {
                        this.RespondPrimary(imId, "Not enough arguments to 'lock' command");
                    }
                    else
                    {
                        if (AppSettings.ProjectorName == args[2])
                        {
                            if (this.m_Manager.SetSpecificClient(args[1]))
                            {
                                this.Respond(primaryChannelId,
                                    "Locking " + AppSettings.ProjectorName + " projector to show @" + args[1] + 
                                    "'s screen for the next 10 minutes at @" + user + "'s request");
                                this.m_Manager.Lock(args[1], user);
                            }
                            else
                            {
                                this.Respond(imId,
                                    "There are no JamCast clients associated with the account name @" + args[1] + " on the " + AppSettings.ProjectorName + " projector");
                            }
                        }
                    }
                    break;
                case "unlock":
                    if (args.Length < 2)
                    {
                        this.RespondPrimary(imId, "Not enough arguments to 'unlock' command");
                    }
                    else
                    {
                        if (AppSettings.ProjectorName == args[2])
                        {
                            if (this.m_Manager.IsLocked)
                            {
                                this.m_Manager.Unlock();
                                this.Respond(primaryChannelId,
                                    AppSettings.ProjectorName + " projector is now unlocked at @" + user + "'s request; projector " +
                                    "will now cycle through random screens");
                            }
                            else
                            {
                                this.Respond(imId,
                                    AppSettings.ProjectorName + " projector is not currently locked");
                            }
                        }
                    }
                    break;
                default:
                    this.RespondPrimary(imId, @"
To control JamCast, send messages (without a leading slash) to this user.  The 
following commands are recognised:

share - make your screen the next one on all the projectors
share [name] - make another users screen show on all of the projectors
share [name] [projector] - show another users screen on a particular projector

lock [name] [projector] - lock a projector to show only a given person's screen (maximum 10 minutes)
unlock [projector] - unlock a projector

projectors - list all of the registered projectors
users - list all known users (this shows only users known by the primary projector)
users [projector] - list all known users by this projector");
                    break;
            }
        }

        private void RespondPrimary(string imOrChannelId, string message)
        {
            if (AppSettings.IsPrimary)
            {
                //var content = JsonConvert.SerializeObject(new
                //{
                //    id = 1,
                //    type = "message",
                //    channel = imOrChannelId,
                //    text = message.Trim(),
                //});
                this.p_Slack.SendMessage(imOrChannelId, message);
            }
        }

        private void Respond(string imOrChannelId, string message)
        {
            //var content = JsonConvert.SerializeObject(new
            //{
            //    id = 1,
            //    type = "message",
            //    channel = imOrChannelId,
            //    text = message.Trim(),
            //});
            this.p_Slack.SendMessage(imOrChannelId, message);
        }

        public List<string> UpdateAndGetChat()
        {
            string message;
            if (this.m_MessagesQueued.TryDequeue(out message))
            {
                this.m_Messages.Add(message);
            }

            if (this.m_Messages.Count > 30)
            {
                
            }

            return this.m_Messages;
        }
    }
}
