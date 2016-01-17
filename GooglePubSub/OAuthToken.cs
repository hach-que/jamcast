using System;

namespace GooglePubSub
{
    /// <summary>
    /// Represents an OAuth token provided by a remote server.
    /// <para>
    /// Both the Google Cloud Storage and Google Cloud Pub/Sub APIs require
    /// an OAuth token to be provided as "Authorization: Bearer &lt;token&gt;"
    /// header in the HTTP request.  However for service authorization, this
    /// token is quite complex to calculate, and requires deriving a token
    /// from a cryptographic public/private key pair that is not only provided
    /// in OpenSSL format (which is hard to parse from C#), but is also used
    /// to sign a complex header that is then using in a handshake with the
    /// token server.
    /// </para>
    /// <para>
    /// When you calculate an OAuth token from the service credentials, you can
    /// also ask for any scopes you like.  However, we don't want to provide
    /// desktop clients (the bootstrap, controller, etc.) with access to Datastore;
    /// we want to explicitly limit them to Storage and Pub/Sub for JamCast
    /// operation.
    /// </para>
    /// <para>
    /// To avoid this complexity, and to limit the scopes that are available, usage
    /// of the GooglePubSub library requires making a request to an external server
    /// that holds the service keys, and that performs the authorization request to
    /// obtain an OAuth token for you.  On this server you can use the PHP libraries
    /// to obtain such a token, which then needs to be provided back to this client
    /// library.  That token is stored in this data structure.
    /// </para>
    /// </summary>
    public class OAuthToken
    {
        /// <summary>
        /// The access token that is used in the HTTP Authorization header that is
        /// sent on all requests.
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// The UTC timestamp that indicates when the access token expires.  If the
        /// current UTC time is later than this time, another request will be made to
        /// the server to obtain a new access token, otherwise a cached value will be
        /// used after the first request.
        /// </summary>
        public DateTime ExpiryUtc { get; set; }
    }
}