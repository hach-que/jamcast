using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TweetSharp;
using System.Windows.Forms;
using System.Threading;

namespace JamCast
{
    class Twitter
    {
        private TwitterService m_Service = null;
        private TwitterSearchResult m_SearchResult = null;
        private System.Windows.Forms.Timer m_RefreshTimer = new System.Windows.Forms.Timer();
        private object m_ThreadLock = new object();
        private List<TwitterSearchStatus> m_StatusList = new List<TwitterSearchStatus>();
        private int m_StatusCount = 0;

        public Twitter()
        {
            this.m_Service = new TwitterService(
                "HIDDEN",
                "HIDDEN",
                "HIDDEN",
                "HIDDEN"
                );

            // Refresh now.
            this.m_RefreshTimer_Tick(this, new EventArgs());

            // Start timer.
            this.m_RefreshTimer.Tick += new EventHandler(m_RefreshTimer_Tick);
            this.m_RefreshTimer.Interval = 60000;
            this.m_RefreshTimer.Start();
        }

        // Refresh the Twitter result.
        private void m_RefreshTimer_Tick(object sender, EventArgs e)
        {
            Thread t = new Thread(() =>
                {
                    this.m_SearchResult = this.m_Service.Search("#ggj11");
                    lock (this.m_ThreadLock)
                    {
                        this.m_StatusCount = this.m_SearchResult.Statuses.Count();
                        this.m_StatusList.Clear();
                        foreach (TwitterSearchStatus tss in this.m_SearchResult.Statuses)
                            this.m_StatusList.Add(tss);
                    }
                });
            t.IsBackground = false;
            t.Start();
        }

        public TwitterSearchStatus Get(int i)
        {
            lock (this.m_ThreadLock)
            {
                return this.m_StatusList[i];
            }
        }

        public int Total
        {
            get { return this.m_StatusCount; }
        }
    }
}
