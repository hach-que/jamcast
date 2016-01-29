﻿using System;
using System.Collections.Generic;
using System.Compat.Web;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using GooglePubSub;
using NetCast;
using NetCast.Messages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TweetSharp;
using Timer = System.Windows.Forms.Timer;

namespace JamCast
{
    public class Manager
    {
        private Broadcast m_Broadcast = null;
        private List<Client> m_Clients = new List<Client>();
        private Timer m_RefreshTimer = null;
        private Timer m_CycleTimer = null;
        private Queue p_NetCast = null;
        private int p_CurrentClient = 0;
        private string p_CurrentClientName = "Unknown!";
        private Twitter m_Twitter = null;
        private int p_TweetID = 0;
        private Timer p_TweetTimer = null;
        private Thread _pubSubThread;

        /// <summary>
        /// Starts the manager cycle.
        /// </summary>
        public void Run()
		{
			// Initalize Twitter.
			this.InitalizeTwitter();

            // Initialize Cloud Pub/Sub.
            this.InitializeCloudPubSub();

            // Initalize everything.
            this.InitalizeBroadcast();
            this.InitalizeTimers();

            // Capture the application exit event.
            Application.ApplicationExit += new EventHandler(Application_ApplicationExit);

            // Start the NetCast listener.
            this.p_NetCast = new Queue(13000, 13001);
            this.p_NetCast.OnReceived += new EventHandler<MessageEventArgs>(p_NetCast_OnReceived);
            
            // Start the application message loop.
            Application.Run();
        }

        private void InitializeCloudPubSub()
        {
            _pubSubThread = new Thread(() =>
            {
                var path = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "JamCast");
                Directory.CreateDirectory(path);

                var guid = Guid.NewGuid();
                using (var reader = new StreamReader(Path.Combine(path, "guid.txt")))
                {
                    guid = Guid.Parse(reader.ReadToEnd().Trim());
                }

                var pubsub = new PubSub(AppSettings.GoogleCloudProjectID, () => GetOAuthTokenFromEndpoint(AppSettings.GoogleCloudOAuthEndpointURL));
                pubsub.CreateTopic("projector-client-ips");
                pubsub.Subscribe("projector-client-ips", "projector-" + guid);
                try
                {
                    while (true)
                    {
                        try
                        {
                            var messages = pubsub.Poll(10, false);
                            foreach (var message in messages)
                            {
                                message.Acknowledge();

                                try
                                {
                                    var m =
                                        JsonConvert.DeserializeObject<dynamic>(
                                            Encoding.ASCII.GetString(Convert.FromBase64String(message.Data)));

                                    if ((string) m.Operation == "add-clients")
                                    {
                                        foreach (var client in ((JArray) m.Clients).Select(x => (string) x))
                                        {
                                            // TODO: AddClientExplicitly();
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine(ex);

                                    try
                                    {
                                        using (var writer = new StreamWriter(Path.Combine(path, "projector-error-log.txt"), true))
                                        {
                                            writer.WriteLine("Recoverable on inner exception:");
                                            writer.WriteLine(ex.ToString());
                                        }
                                    }
                                    catch { }
                                }
                            }

                            Thread.Sleep(1);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex);

                            try
                            {
                                using (var writer = new StreamWriter(Path.Combine(path, "projector-error-log.txt"), true))
                                {
                                    writer.WriteLine("Recoverable on outer exception:");
                                    writer.WriteLine(ex.ToString());
                                }
                            }
                            catch { }
                        }
                    }
                }
                finally
                {
                    pubsub.Unsubscribe("projector-client-ips", "projector-" + guid);
                }
            });
            _pubSubThread.IsBackground = true;
            _pubSubThread.Start();
        }

        private static OAuthToken GetOAuthTokenFromEndpoint(string currentEndpoint)
        {
            while (true)
            {
                try
                {
                    var client = new WebClient();
                    var jsonResult = client.DownloadString(currentEndpoint);
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

        private void InitalizeTwitter()
        {
            this.m_Twitter = new Twitter();
            this.p_TweetID = 0;
            this.p_TweetTimer = new Timer();
            this.p_TweetTimer.Tick += new EventHandler(p_TweetTimer_Tick);
            this.p_TweetTimer.Interval = 3000;
            this.p_TweetTimer.Start();
        }

        private void p_TweetTimer_Tick(object sender, EventArgs e)
        {
            this.p_TweetID += 1;
            if (this.p_TweetID == this.m_Twitter.Total)
                this.p_TweetID = 0;
        }

        private void Application_ApplicationExit(object sender, EventArgs e)
        {
            this.p_NetCast.Stop();
        }

        public string GetTweetStream()
        {
            string t = "";
            for (int i = 0; i < this.m_Twitter.Total; i += 1)
            {
                TwitterSearchStatus tss = this.m_Twitter.Get(i);
                t += "*" + HttpUtility.HtmlDecode(tss.Author.ScreenName) + "* - " + HttpUtility.HtmlDecode(tss.Text) + "                    ";
            }
            return t;
        }

        /// <summary>
        /// This event is fired when a network message has been received.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void p_NetCast_OnReceived(object sender, MessageEventArgs e)
        {
            if (e.Message is ClientServiceStartingMessage)
            {
                AddClientExplicitly(e.Message.Source, (e.Message as ClientServiceStartingMessage).Name);
            }
            else if (e.Message is ClientServiceStoppingMessage)
            {
                this.m_Clients.RemoveAll((nc) =>
                {
                    return (nc.Source == e.Message.Source);
                });
            }
        }

        public void AddClientExplicitly(IPEndPoint source, string name)
        {
            var nc = new Client(this.p_NetCast, m_Broadcast, source, name);
            foreach (var snc in this.m_Clients)
            {
                if (snc.Source.Address.Equals(nc.Source.Address))
                    return;
            }
            nc.OnDisconnected += new EventHandler(nc_OnDisconnected);
            this.m_Clients.Add(nc);

            if (this.m_Clients.Count == 1)
            {
                nc.Start();
            }
        }

        /// <summary>
        /// This event is fired when a client disconnects.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void nc_OnDisconnected(object sender, EventArgs e)
        {
            this.m_Clients.Remove(sender as Client);
        }
        
        /// <summary>
        /// Initalizes and shows the broadcast form.
        /// </summary>
        private void InitalizeBroadcast()
        {
            this.m_Broadcast = new Broadcast(this);
            this.m_Broadcast.Show();
            this.m_Broadcast.FormClosed += (sender, e) =>
                {
                    // Shutdown all network connections and close the app.
                    Application.Exit();
                };
        }

        /// <summary>
        /// Initalizes the refresh timer, which is used to repaint
        /// the form at 60 FPS.
        /// </summary>
        private void InitalizeTimers()
        {
            // Set up the refresh timer.
			#if PLATFORM_MACOS
			var thread = new Thread(new ThreadStart(() =>
				{
					while (true) {
						this.m_Broadcast.Invalidate();
						Thread.Sleep(1000 / 60);
					}
				}));
			thread.IsBackground = true;
			thread.Start();
			#else
            this.m_RefreshTimer = new Timer();
            this.m_RefreshTimer.Interval = 1000 / 60;
            this.m_RefreshTimer.Tick += (sender, e) =>
                {
                    this.m_Broadcast.Invalidate();
                };
            this.m_RefreshTimer.Start();
		    #endif

			// Set up the cycle timer.
			this.m_CycleTimer = new Timer();
            this.m_CycleTimer.Interval = 30000;
            this.m_CycleTimer.Tick += (sender, e) =>
            {
                if (this.IsLocked)
                {
                    this.m_CycleTimer.Interval = 30000;
                    this.IsLocked = false;
                    this.LockedClientName = null;
                    this.LockingUserName = null;
                }

                NextClient();
            };
            this.m_CycleTimer.Start();
        }

        public void NextClient()
        {
            BitmapTracker.Purge();

            var oldClient = this.p_CurrentClient;

            this.p_CurrentClient += 1;

            if (this.p_CurrentClient >= this.m_Clients.Count)
                this.p_CurrentClient = 0;

            if (this.p_CurrentClient != oldClient)
            {
                if (oldClient < this.m_Clients.Count)
                {
                    if (this.m_Clients[oldClient] != null)
                    {
                        this.m_Clients[oldClient].Stop();
                    }
                }
                if (this.p_CurrentClient < this.m_Clients.Count)
                {
                    if (this.m_Clients[this.p_CurrentClient] != null)
                    {
                        this.m_Clients[this.p_CurrentClient].Start();
                    }
                }
            }
        }

        /// <summary>
        /// The current client ID.
        /// </summary>
        public int CurrentClient
        {
            get { return this.p_CurrentClient; }
            set { this.p_CurrentClient = value; this.p_CurrentClientName = "Unknown!"; }
        }

        /// <summary>
        /// The current client name.
        /// </summary>
        public string CurrentClientName
        {
            get { return this.p_CurrentClientName; }
        }

        public bool SetSpecificClient(string name)
        {
            for (var i = 0; i < this.m_Clients.Count; i++)
            {
                if (this.m_Clients[i].Name == name)
                {
                    this.p_CurrentClient = i;
                    this.p_CurrentClientName = name;
                    this.m_CycleTimer.Stop();
                    this.m_CycleTimer.Start();
                    return true;
                }
            }

            return false;
        }

        public string[] GetClientNames()
        {
            return this.m_Clients.Select(x => x.Name).ToArray();
        }

        public bool IsLocked { get; set; }
        public string LockedClientName { get; set; }
        public string LockingUserName { get; set; }

        public Client CurrentClientObject
        {
            get { return this.p_CurrentClient < this.m_Clients.Count ? this.m_Clients[this.p_CurrentClient] : null; }
        }

        public void Lock(string target, string lockingUser)
        {
            this.IsLocked = true;
            this.LockedClientName = target;
            this.LockingUserName = lockingUser;
            this.m_CycleTimer.Stop();
            this.m_CycleTimer.Interval = 1000*60 * 10;
            this.m_CycleTimer.Start();
        }

        public void Unlock()
        {
            this.m_CycleTimer.Stop();
            this.m_CycleTimer.Interval = 30000;
            this.m_CycleTimer.Start();
            this.IsLocked = false;
            this.LockedClientName = null;
            this.LockingUserName = null;
        }
    }
}
