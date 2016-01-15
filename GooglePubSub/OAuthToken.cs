using System;

namespace GooglePubSub
{
    public class OAuthToken
    {
        public string AccessToken { get; set; }

        public DateTime ExpiryUtc { get; set; }
    }
}