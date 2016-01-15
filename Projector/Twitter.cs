using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using TweetSharp;
using Timer = System.Windows.Forms.Timer;

namespace JamCast
{
    class Twitter
    {
        private readonly TwitterService m_Service = null;
        private TwitterSearchResult m_SearchResult = null;
        private readonly Timer m_RefreshTimer = new Timer();
        private readonly object m_ThreadLock = new object();
        private readonly List<TwitterSearchStatus> m_StatusList = new List<TwitterSearchStatus>();
        private int m_StatusCount = 0;

        public Twitter()
        {
            this.m_Service = new TwitterService(
                AppSettings.TwitterConsumerKey,
                AppSettings.TwitterConsumerSecret,
                AppSettings.TwitterOAuthToken,
                AppSettings.TwitterOAuthSecret
                );

            // Refresh now.
            this.m_RefreshTimer_Tick(this, new EventArgs());

            // Start timer.
            this.m_RefreshTimer.Tick += new EventHandler(m_RefreshTimer_Tick);
            this.m_RefreshTimer.Interval = 60000;
            this.m_RefreshTimer.Start();
        }

        [DataContract]
        private class Status
        {
            [DataMember(Name = "text")]
            public string Text;

            [DataMember(Name = "user")]
            public StatusUser User;
        }

        [DataContract]
        private class StatusUser
        {
            [DataMember(Name = "name")]
            public string Name;
        }

        // Refresh the Twitter result.
        private void m_RefreshTimer_Tick(object sender, EventArgs e)
        {
            if (AppSettings.TwitterEnabled)
            {
                Thread t = new Thread(() =>
                {
                    try
                    {
                        this.m_SearchResult = this.m_Service.Search(AppSettings.TwitterSearchQuery);
                        lock (this.m_ThreadLock)
                        {
                            this.m_StatusCount = this.m_SearchResult.Statuses.Count();
                            this.m_StatusList.Clear();
                            foreach (TwitterSearchStatus tss in this.m_SearchResult.Statuses)
                                this.m_StatusList.Add(tss);
                        }
                    }
                    catch (Exception)
                    {
                    }
                });
                t.IsBackground = false;
                t.Start();
            }
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
