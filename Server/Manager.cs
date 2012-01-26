using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using JamCast.Clients;
using System.Drawing;
using NetCast;
using NetCast.Messages;
using System.Threading;
using System.Compat.Web;

namespace JamCast
{
    public class Manager
    {
        private Broadcast m_Broadcast = null;
        private List<Client> m_Clients = new List<Client>();
        private System.Windows.Forms.Timer m_RefreshTimer = null;
        private System.Windows.Forms.Timer m_CycleTimer = null;
        private NetCast.Queue p_NetCast = null;
        private int p_CurrentClient = 0;
        private string p_CurrentClientName = "Unknown!";
        private Twitter m_Twitter = null;
        private int p_TweetID = 0;
        private System.Windows.Forms.Timer p_TweetTimer = null;
        private List<string> m_Chat = new List<string>();

        /// <summary>
        /// Starts the manager cycle.
        /// </summary>
        public void Run()
        {
            // Initalize everything.
            this.InitalizeBroadcast();
            this.InitalizeTimers();

            // Initalize Twitter.
            this.InitalizeTwitter();

            // Capture the application exit event.
            Application.ApplicationExit += new EventHandler(Application_ApplicationExit);

            // Start the NetCast listener.
            this.p_NetCast = new NetCast.Queue(13000, 13001);
            this.p_NetCast.OnReceived += new EventHandler<MessageEventArgs>(p_NetCast_OnReceived);

            // Start the application message loop.
            Application.Run();
        }

        private void InitalizeTwitter()
        {
            this.m_Twitter = new Twitter();
            this.p_TweetID = 0;
            this.p_TweetTimer = new System.Windows.Forms.Timer();
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
                TweetSharp.TwitterSearchStatus tss = this.m_Twitter.Get(i);
                t += "*" + HttpUtility.HtmlDecode(tss.Author.ScreenName) + "* - " + HttpUtility.HtmlDecode(tss.Text) + "                    ";
            }
            return t;
        }

        public TweetSharp.TwitterSearchStatus GetTweet()
        {
            return this.m_Twitter.Get(this.p_TweetID);
        }

        private void UpdateChat()
        {
            string s = this.m_Twitter.GetNextChatMessage();
            while (s != null)
            {
                this.m_Chat.Insert(0, s);
                if (this.m_Chat.Count > 20)
                    this.m_Chat.RemoveAt(20);

                s = this.m_Twitter.GetNextChatMessage();
            }
        }

        public List<string> GetChatStream()
        {
            return this.m_Chat;
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
                // Search for existing client in list.
                NetworkClient nc = new NetworkClient(this.p_NetCast, e.Message.Source, (e.Message as ClientServiceStartingMessage).Name);
                foreach (NetworkClient snc in this.m_Clients)
                {
                    if (snc.Source.Address.Equals(nc.Source.Address))
                        return;
                }
                nc.OnDisconnected += new EventHandler(nc_OnDisconnected);
                this.m_Clients.Add(nc);
            }
            else if (e.Message is ClientServiceStoppingMessage)
            {
                this.m_Clients.RemoveAll((nc) =>
                    {
                        if (nc is NetworkClient)
                            return ((nc as NetworkClient).Source == e.Message.Source);
                        else
                            return false;
                    });
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
        /// The current bitmap data to be broadcasted.
        /// </summary>
        public Bitmap Screen
        {
            get
            {
                // For the moment, grab the first client and show it's screen.
                // Also implement some caching here since GetScreen() should be asynchronous.
                if (this.m_Clients.Count > 0)
                {
                    // Clamp values.
                    if (this.p_CurrentClient >= this.m_Clients.Count)
                        this.p_CurrentClient = this.m_Clients.Count - 1;
                    else if (this.p_CurrentClient < 0)
                        this.p_CurrentClient = 0;

                    // Get screen.
                    Bitmap b = this.m_Clients[this.p_CurrentClient].Screen;
                    this.p_CurrentClientName = this.m_Clients[this.p_CurrentClient].Name;
                    Thread t = new Thread(() =>
                        {
                            try
                            {
                                this.m_Clients[this.p_CurrentClient].Refresh();
                            }
                            catch (ArgumentOutOfRangeException)
                            {
                                this.p_CurrentClient = 0;
                            }
                        });
                    t.IsBackground = true;
                    t.Start();
                    return b;
                }                    
                else
                    return null;
            }
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
            this.m_RefreshTimer = new System.Windows.Forms.Timer();
            this.m_RefreshTimer.Interval = 1000 / 60;
            this.m_RefreshTimer.Tick += (sender, e) =>
                {
                    this.UpdateChat();
                    this.m_Broadcast.Invalidate();
                };
            this.m_RefreshTimer.Start();

            // Set up the cycle timer.
            this.m_CycleTimer = new System.Windows.Forms.Timer();
            this.m_CycleTimer.Interval = 30000;
            this.m_CycleTimer.Tick += (sender, e) =>
            {
                this.p_CurrentClient += 1;

                if (this.p_CurrentClient >= this.m_Clients.Count)
                    this.p_CurrentClient = 0;
            };
            this.m_CycleTimer.Start();
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
    }
}
