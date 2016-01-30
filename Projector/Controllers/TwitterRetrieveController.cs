using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

using JamCast;

using Projector.Controllers;

using TweetSharp;

using Timer = System.Windows.Forms.Timer;

namespace Projector
{
    public class TwitterRetrieveController : IController
    {
        private TwitterService m_Service = null;
        private TwitterSearchResult m_SearchResult = null;
        private readonly Timer m_RefreshTimer = new Timer();
        private readonly object m_ThreadLock = new object();
        private readonly List<TwitterSearchStatus> m_StatusList = new List<TwitterSearchStatus>();
        private int m_StatusCount = 0;

        private bool _initialized = false;

        private DateTime? _lastQuery;

        public void Update(PubSubController pubSubController, TwitterRetrieveController twitterRetrieveController, TwitterProcessingController twitterProcessingController, ClientListController clientListController, ClientSelectionController clientSelectionController, StreamController streamController, FfmpegProcessController ffmpegProcessController)
        {
            if (!AppSettings.TwitterEnabled)
            {
                return;
            }

            if (!this._initialized)
            {
                Init();
            }

            if (this._lastQuery == null || this._lastQuery.Value < DateTime.UtcNow.AddMinutes(-1))
            {
                this._lastQuery = DateTime.UtcNow;
                Task.Run(() => {
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
            }
        }

        private void Init()
        {
            this.m_Service = new TwitterService(
                AppSettings.TwitterConsumerKey,
                AppSettings.TwitterConsumerSecret,
                AppSettings.TwitterOAuthToken,
                AppSettings.TwitterOAuthSecret
                );
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