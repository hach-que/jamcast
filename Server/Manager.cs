using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using JamCast.Clients;
using System.Drawing;
using NetCast;
using NetCast.Messages;

namespace JamCast
{
    public class Manager
    {
        private Broadcast m_Broadcast = null;
        private List<Client> m_Clients = new List<Client>();
        private Timer m_RefreshTimer = null;
        private Timer m_CycleTimer = null;
        private NetCast.Queue p_NetCast = null;
        private int p_CurrentClient = 0;

        /// <summary>
        /// Starts the manager cycle.
        /// </summary>
        public void Run()
        {
            // Initalize everything.
            this.InitalizeBroadcast();
            this.InitalizeTimers();

            // Start the NetCast listener.
            this.p_NetCast = new NetCast.Queue(13000, 13001);
            this.p_NetCast.OnReceived += new EventHandler<MessageEventArgs>(p_NetCast_OnReceived);

            // Start the application message loop.
            Application.Run();
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
                NetworkClient nc = new NetworkClient(this.p_NetCast, e.Message.Source);
                nc.OnDisconnected += new EventHandler(nc_OnDisconnected);
                this.m_Clients.Add(nc);
            }
            else if (e.Message is CountdownBroadcastMessage)
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
                    return this.m_Clients[this.p_CurrentClient].GetScreen();
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
            this.m_RefreshTimer = new Timer();
            this.m_RefreshTimer.Interval = 1000 / 60;
            this.m_RefreshTimer.Tick += (sender, e) =>
                {
                    this.m_Broadcast.Invalidate();
                };
            this.m_RefreshTimer.Start();

            // Set up the cycle timer.
            this.m_CycleTimer = new Timer();
            this.m_CycleTimer.Interval = 3000;
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
            set { this.p_CurrentClient = value; }
        }
    }
}
