using System;
using System.Collections.Generic;
using System.Compat.Web;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Projector.Controllers;

using TweetSharp;

namespace Projector
{
    public class TwitterProcessingController : IController
    {
        private bool _initialized;

        private TwitterRetrieveController _twitterRetrieveController;

        private int _tweetID = 0;

        private DateTime? _lastTweetUpdate;

        public void Update(PubSubController pubSubController, TwitterRetrieveController twitterRetrieveController, TwitterProcessingController twitterProcessingController, ClientListController clientListController, ClientSelectionController clientSelectionController, StreamController streamController, FfmpegProcessController ffmpegProcessController)
        {
            _twitterRetrieveController = twitterRetrieveController;

            if (!this._initialized)
            {
                this._tweetID = 0;
            }

            if (this._lastTweetUpdate == null || this._lastTweetUpdate.Value < DateTime.UtcNow.AddSeconds(-3))
            {
                this._tweetID += 1;
                if (this._tweetID >= this._twitterRetrieveController.Total)
                {
                    this._tweetID = 0;
                }
            }
        }

        public string GetTweetStream()
        {
            string t = "";
            for (int i = 0; i < this._twitterRetrieveController.Total; i += 1)
            {
                TwitterSearchStatus tss = this._twitterRetrieveController.Get(i);
                t += "*" + HttpUtility.HtmlDecode(tss.Author.ScreenName) + "* - " + HttpUtility.HtmlDecode(tss.Text) + "                    ";
            }
            return t;
        }
    }
}
