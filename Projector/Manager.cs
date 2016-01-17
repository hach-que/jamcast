using System;
using System.Collections.Generic;
using System.Compat.Web;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using NetCast;
using NetCast.Messages;
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
        private SlackController m_Slack;

        /// <summary>
        /// Starts the manager cycle.
        /// </summary>
        public void Run()
		{
			// Initalize Twitter.
			this.InitalizeTwitter();

			// Initialize Slack.
			this.InitializeSlack();

            // Initalize everything.
            this.InitalizeBroadcast();
            this.InitalizeTimers();

            // Capture the application exit event.
            Application.ApplicationExit += new EventHandler(Application_ApplicationExit);

            // Start the NetCast listener.
            this.p_NetCast = new Queue(13000, 13001);
            this.p_NetCast.OnReceived += new EventHandler<MessageEventArgs>(p_NetCast_OnReceived);

            AddClientExplicitly(new IPEndPoint(IPAddress.Parse("10.1.1.28"), 12001), "test client");

            // Start the application message loop.
            Application.Run();
        }

        private void InitializeSlack()
        {
            this.m_Slack = new SlackController(this);

            this.IsLocked = false;
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

        public List<string> GetChatStream()
        {
            return this.m_Slack.UpdateAndGetChat();
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
