using System;
using System.Net;

namespace GooglePubSub
{
    /// <summary>
    /// This is a modified web client that only waits 10 seconds for a response.
    /// </summary>
    public class ShortTimeoutWebClient : WebClient
    {
        protected override WebRequest GetWebRequest(Uri uri)
        {
            var w = base.GetWebRequest(uri);
            w.Timeout = 20*1000;
            return w;
        }
    }
}