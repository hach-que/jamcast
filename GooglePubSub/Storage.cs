using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GooglePubSub
{
    /// <summary>
    /// A client class for using the Google Cloud Storage system.
    /// </summary>
    public class Storage
    {
        /// <summary>
        /// The Google Cloud Storage bucket name where data is stored.
        /// </summary>
        private readonly string _bucketName;

        /// <summary>
        /// A callback which is used to obtain the OAuth token sent on requests.  To
        /// use this library, you must provide this function.
        /// </summary>
        private Func<OAuthToken> _getToken;

        /// <summary>
        /// The cached OAuth token which is used until it expires.
        /// </summary>
        private OAuthToken _token;

        /// <summary>
        /// Creates a client for the Google Cloud Storage system.
        /// </summary>
        /// <param name="bucketName">
        /// A Google Cloud bucket name like "melbourne-global-game-jam-16".
        /// </param>
        /// <param name="getBearerToken">
        /// A callback which provides an OAuth token for authorizing requests with
        /// Google Cloud.  You will most likely need to implement a remote server that
        /// can generate the OAuth token using the Google Cloud PHP libraries.  Refer
        /// to <see cref="OAuthToken"/> for more information.
        /// </param>
        public Storage(string bucketName, Func<OAuthToken> getBearerToken)
        {
            _bucketName = bucketName;
            _getToken = getBearerToken;

            CheckToken();
        }
        
        /// <summary>
        /// Checks the currently cached token in <see cref="_token"/> and ensures that it
        /// is still valid.  If it is not valid, uses the token callback that was provided
        /// in the constructor to obtain a new token.
        /// </summary>
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

        /// <summary>
        /// Uploads the given byte array as a public file to the Google Cloud Storage system.  The
        /// file that is uploaded will be marked as publically accessible.  Returns the URL that
        /// can be used to download the file.
        /// </summary>
        /// <param name="fileData">The file data as an array of bytes.</param>
        /// <param name="name">The filename to use to store the file.</param>
        /// <param name="progress">A callback that reports the upload progress.</param>
        /// <returns>The publically accessible URL that is used to download the file.</returns>
        public async Task<string> Upload(byte[] fileData, string name, Action<double> progress)
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

            var client = new AccurateWebClient(fileData.Length);
            client.Headers.Add("Authorization", "Bearer " + _token.AccessToken);
            client.UploadProgressChanged += (sender, args) =>
            {
                progress(args.BytesSent/(double) args.TotalBytesToSend);
            };
            var result = await client.UploadDataTaskAsync(uploadLocation, "PUT", fileData);

            using (var s = new StreamReader(new MemoryStream(result)))
            {
                var json = s.ReadToEnd();
                return JsonConvert.DeserializeObject<dynamic>(json).mediaLink;
            }
        }

        private class AccurateWebClient : WebClient
        {
            private readonly int m_ContentLength;

            public AccurateWebClient(int length)
            {
                this.m_ContentLength = length;
            }

            protected override WebRequest GetWebRequest(Uri address)
            {
                var req = base.GetWebRequest(address) as HttpWebRequest;
                req.AllowWriteStreamBuffering = false;
                req.ContentLength = this.m_ContentLength;
                return req;
            }
        }
    }
}
