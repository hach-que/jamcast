using System;
using System.Net;

namespace GooglePubSub
{
    /// <summary>
    /// This is a modified web client that only waits a certain amont of time for a response.
    /// </summary>
    public class ShortTimeoutWebClient : WebClient
    {
        private readonly int _seconds;

        public ShortTimeoutWebClient(int seconds)
        {
            _seconds = seconds;
        }

        protected override WebRequest GetWebRequest(Uri uri)
        {
            var w = base.GetWebRequest(uri);
            w.Timeout = _seconds*1000;
            return w;
        }
    }
}