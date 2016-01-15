using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GooglePubSub
{
    public class Storage
    {
        private readonly string _bucketName;

        private Func<OAuthToken> _getToken;
        private OAuthToken _token;

        /// <summary>
        /// The API for Google Cloud Storage.
        /// </summary>
        /// <param name="bucketName">
        /// A project URL like "melbourne-global-game-jam-16"
        /// </param>
        /// <param name="getBearerToken">
        /// The API bearer token to use.
        /// </param>
        public Storage(string bucketName, Func<OAuthToken> getBearerToken)
        {
            _bucketName = bucketName;
            _getToken = getBearerToken;

            CheckToken();
        }

        private WebClient MakeClient()
        {
            var client = new WebClient();
            client.Headers["Authorization"] = "Bearer " + _token.AccessToken;
            client.Headers["Content-Type"] = "application/json";
            return client;
        }

        private void CheckToken()
        {
            if (_token == null)
            {
                _token = _getToken();
            }
            if (_token.ExpiryUtc < DateTime.UtcNow)
            {
                _token = _getToken();
            }
        }

        public string Upload(byte[] fileData, string name, Action<double> progress)
        {
            var request = new
            {
                name = name,
                acl = new[]
                {
                    new
                    {
                        role = "READER",
                        entity = "allUsers",
                    }
                }
            };
            var requestSerialized = JsonConvert.SerializeObject(request);

            CheckToken();

            var webRequest = (HttpWebRequest)WebRequest.Create("https://www.googleapis.com/upload/storage/v1/b/" + _bucketName + "/o?uploadType=resumable");
            webRequest.Method = "POST";
            webRequest.ContentType = "application/json";
            webRequest.Headers.Add("Authorization", "Bearer " + _token.AccessToken);
            var buffer = Encoding.ASCII.GetBytes(requestSerialized);
            using (var s = webRequest.GetRequestStream())
            {
                s.Write(buffer, 0, buffer.Length);
            }
            var response = webRequest.GetResponse();
            var uploadLocation = response.Headers["Location"];

            var client = MakeClient();
            client.UploadProgressChanged += (sender, args) =>
            {
                progress(args.BytesSent/(double) args.TotalBytesToSend);
            };
            var result = client.UploadData(uploadLocation, "PUT", fileData);

            string downloadLink;
            using (var s = new StreamReader(new MemoryStream(result)))
            {
                var json = s.ReadToEnd();
                return JsonConvert.DeserializeObject<dynamic>(json).mediaLink;
            }
        }

        
    }
}
