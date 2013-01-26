using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TweetSharp;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

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
        private StreamReader m_ChatStream = null;
        private Thread m_ChatThread = null;
        private Queue<string> m_ChatQueue = new Queue<string>();
        private object m_ChatLock = new object();

        public Twitter()
        {
            this.m_Service = new TwitterService(
                AppSettings.ConsumerKey,
                AppSettings.ConsumerSecret,
                AppSettings.OAuthToken,
                AppSettings.OAuthSecret
                );

            // Create thread for streaming API.
            this.StartStreaming();

            // Refresh now.
            this.m_RefreshTimer_Tick(this, new EventArgs());

            // Start timer.
            this.m_RefreshTimer.Tick += new EventHandler(m_RefreshTimer_Tick);
            this.m_RefreshTimer.Interval = 60000;
            this.m_RefreshTimer.Start();
        }

        void m_ChatThread_Run()
        {
            Encoding encode = Encoding.GetEncoding("utf-8");
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Status));

            while (!this.m_ChatStream.EndOfStream)
            {
                string l = this.m_ChatStream.ReadLine();
                if (l.Trim() == "") continue;
                Status s = null;
                try
                {
                    s = ser.ReadObject(new MemoryStream(encode.GetBytes(l))) as Status;
                }
                catch (Exception) { continue; }
                if (s.Text.ToLower().Contains("@melbournejam"))
                    lock (this.m_ChatLock)
                        this.m_ChatQueue.Enqueue(s.User.Name + ": " + s.Text
                            .Replace("@MelbourneJam", "")
                            .Replace("@melbourneJam", "")
                            .Replace("@Melbournejam", "")
                            .Replace("@melbournejam", "").Trim());
            }
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
            Thread t = new Thread(() =>
                {
                    this.m_SearchResult = this.m_Service.Search("#ggj13");
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

        private void StartStreaming()
        {
            string url = "https://stream.twitter.com/1/statuses/filter.json?track=" + AppSettings.MessagingUser;

            WebRequest request = WebRequest.Create(url);
            request.Credentials = new NetworkCredential(AppSettings.StreamUsername, AppSettings.StreamPassword);
            ServicePointManager.ServerCertificateValidationCallback = (sender, ICertificatePolicy, chain, error) =>
            {
                return true;
            };

            try
            {
                var webResponse = request.GetResponse();

                Encoding encode = Encoding.GetEncoding("utf-8");

                this.m_ChatStream = new StreamReader(webResponse.GetResponseStream(), encode);

                var t = new Thread(m_ChatThread_Run);
                t.IsBackground = true;
                t.Start();
            }
            catch (WebException)
            {
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

        public string GetNextChatMessage()
        {
            lock (this.m_ChatLock)
            {
                if (this.m_ChatQueue.Count == 0)
                    return null;
                return this.m_ChatQueue.Dequeue();
            }
        }
    }
}
