using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GooglePubSub
{
    public class ServiceAccount
    {
        private readonly string _email;
        private string _cachedToken;

        public ServiceAccount(string email, string key)
        {
            _email = email;
        }

        public string GetAPIToken()
        {
            if (_cachedToken != null)
            {
                return _cachedToken;
            }
            /*


            // From https://developers.google.com/identity/protocols/OAuth2ServiceAccount#creatingjwt
            var header = @"{""alg"":""RS256"",""typ"":""JWT""}";
            var base64Header = Convert.ToBase64String(Encoding.ASCII.GetBytes(header));
            var claim = new 
            {
                iss = _email,
                scope = "https://www.googleapis.com/auth/pubsub",
                aud = "https://www.googleapis.com/oauth2/v4/token",
                exp = 
            }*/
        }
    }
}
