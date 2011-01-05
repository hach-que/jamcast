using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using JamCast.Clients;
using System.Drawing;

namespace JamCast
{
    public class Manager
    {
        private Broadcast m_Broadcast = null;
        private List<Client> m_Clients = new List<Client>();
        private Timer m_RefreshTimer = null;

        /// <summary>
        /// Starts the manager cycle.
        /// </summary>
        public void Run()
        {
            // Initalize everything.
            this.InitalizeBroadcast();
            this.InitalizeTimer();

            // TEST: Initalize a SelfClient, which sends the server's screen to it's broadcasting mechanism.
            this.m_Clients.Add(new SelfClient());

            // Start the application message loop.
            Application.Run();
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
                    return this.m_Clients[0].GetScreen();
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
        private void InitalizeTimer()
        {
            this.m_RefreshTimer = new Timer();
            this.m_RefreshTimer.Interval = 1000 / 60;
            this.m_RefreshTimer.Tick += (sender, e) =>
                {
                    this.m_Broadcast.Invalidate();
                };
            this.m_RefreshTimer.Start();
        }
    }
}
