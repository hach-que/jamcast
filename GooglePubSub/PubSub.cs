using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GooglePubSub
{
    /// <summary>
    /// A client class for using the Google Cloud Pub/Sub system.
    /// </summary>
    public class PubSub
    {
        /// <summary>
        /// The project name specified on Google Cloud.  This will be something
        /// like "melbourne-global-game-jam-16".
        /// </summary>
        private readonly string _projectName;

        /// <summary>
        /// A list of subscriptions that we are currently subscribed to.  This is
        /// used when polling so that if we are subscribed to multiple subscriptions,
        /// we can get messages from the first one that returns, regardless of the
        /// subscription order.
        /// </summary>
        private readonly List<string> _subscriptions;

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
        /// Creates a client for the Google Cloud Pub/Sub system.
        /// </summary>
        /// <param name="projectName">
        /// A Google Cloud project name like "melbourne-global-game-jam-16".
        /// </param>
        /// <param name="getBearerToken">
        /// A callback which provides an OAuth token for authorizing requests with
        /// Google Cloud.  You will most likely need to implement a remote server that
        /// can generate the OAuth token using the Google Cloud PHP libraries.  Refer
        /// to <see cref="OAuthToken"/> for more information.
        /// </param>
        public PubSub(string projectName, Func<OAuthToken> getBearerToken)
        {
            _projectName = projectName;
            _subscriptions = new List<string>();
            _getToken = getBearerToken;

            Timeout = 120;
            OperationsRequested = 0;

            CheckToken();
        }

        /// <summary>
        /// The timeout (in seconds) for all Google Cloud Pub/Sub operations.
        /// <para>
        /// The default is 60 seconds.
        /// </para>
        /// </summary>
        public int Timeout { get; set; }

        /// <summary>
        /// The number of Google Cloud Pub/Sub operations performed by this
        /// client.
        /// </summary>
        public int OperationsRequested { get; private set; }

        /// <summary>
        /// Creates a <see cref="WebClient"/> internally that has the correct authorization
        /// header for making requests.
        /// </summary>
        /// <returns>A <see cref="WebClient"/> with the correct header information.</returns>
        private WebClient MakeClient()
        {
            var client = new ShortTimeoutWebClient(Timeout);
            client.Headers["Authorization"] = "Bearer " + _token.AccessToken;
            
            return client;
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
        /// Creates a new Pub/Sub topic with the given name.  A topic can be subscribed to
        /// by clients, e.g. with the <see cref="Subscribe"/> method.
        /// <para>
        /// If the topic already exists, this method does not throw an exception and instead
        /// continues normally (it is idempotent).
        /// </para>
        /// </summary>
        /// <param name="topic">The topic name.</param>
        public void CreateTopic(string topic)
        {
            CheckToken();

            var request = new
            {
                name = "projects/" + _projectName + "/topics/" + topic
            };
            var requestSerialized = JsonConvert.SerializeObject(request);

            try
            {
                MakeRetryableRequest(
                    "https://pubsub.googleapis.com/v1/" + request.name,
                    "PUT",
                    requestSerialized);
            }
            catch (WebException ex)
            {
                var webResponse = ex.Response as HttpWebResponse;
                if (webResponse != null && webResponse.StatusCode == HttpStatusCode.Conflict)
                {
                    // This is fine, the topic already exists.
                    return;
                }

                throw;
            }
        }

        /// <summary>
        /// Creates a new uniquely named subscription to a Pub/Sub topic.
        /// <para>
        /// When we create the subscription in the Pub/Sub system, we internally give
        /// it a name of "t-(topic)-s-(subscription)".  We do this because subscription
        /// names must be unique across the whole system, not just unique within the 
        /// topic they are subscribing to.
        /// </para>
        /// </summary>
        /// <param name="topic">The topic name.</param>
        /// <param name="subscription">
        /// A unique subscription identifier.  This should be unique per instance of the application, while
        /// at the same time, it should not be randomly generated on every invocation (to avoid creating
        /// an infinite number of subscriptions when the application does not exit cleanly).
        /// <para>
        /// For JamCast, we implement this by uniquely creating a GUID and saving it on disk, and then loading
        /// it for every future run on the application.  We use this GUID in all subscriptions, with a prefix
        /// for the application name.
        /// </para>
        /// </param>
        public void Subscribe(string topic, string subscription)
        {
            CheckToken();

            var request = new
            {
                name = "projects/" + _projectName + "/subscriptions/t-" + topic + "-s-" + subscription,
                topic = "projects/" + _projectName + "/topics/" + topic,
                ackDeadlineSeconds = 0,
            };
            var requestSerialized = JsonConvert.SerializeObject(request);
            try
            {
                MakeRetryableRequest(
                    "https://pubsub.googleapis.com/v1/projects/" + _projectName + "/subscriptions/t-" + topic + "-s-" +
                    subscription,
                    "PUT",
                    requestSerialized);
            }
            catch (WebException ex)
            {
                var webResponse = ex.Response as HttpWebResponse;
                if (webResponse != null && webResponse.StatusCode == HttpStatusCode.Conflict)
                {
                    // This is fine, the subscription already exists.
                }
                else
                {
                    throw;
                }
            }
            if (!_subscriptions.Contains("t-" + topic + "-s-" + subscription))
            {
                _subscriptions.Add("t-" + topic + "-s-" + subscription);
            }
        }

        /// <summary>
        /// Polls the Pub/Sub system for new messages, up to a given number of messages per
        /// subscription.  This will poll all topics you have subscribed to.
        /// </summary>
        /// <param name="messagesPerSubscription">
        /// The maximum number of messages per subscription.  From observations, it appears that
        /// message delivery is more responsive when this number is set to something like 10 rather
        /// than 1, potentially because it reduces the amount of locking that must be performed on
        /// Google's servers for clients that are polling.
        /// </param>
        /// <param name="immediate">
        /// Whether to return immediately if there are no new messages.  If this is set to <c>false</c>,
        /// then this method will perform a long poll.
        /// </param>
        /// <returns>
        /// A list of messages that were received.
        /// </returns>
        public List<Message> Poll(int messagesPerSubscription, bool immediate)
        {
            CheckToken();

            Debug.WriteLine("POLL START");

            var messages = new ConcurrentBag<Message>();

            if (_subscriptions.Count == 0)
            {
                throw new InvalidOperationException("You can't poll when you're not subscribed to anything!");
            }

            var webClients = new ConcurrentBag<WebClient>();
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            var tasks = (from topicSubscription in _subscriptions
                select Task.Run(async () =>
                {
                    var request = new
                    {
                        maxMessages = messagesPerSubscription,
                        returnImmediately = immediate,
                    };
                    var requestSerialized = JsonConvert.SerializeObject(request);
                    try
                    {
                        Debug.WriteLine("POLL TASK STARTED OPERATION");

                        string responseSerialized;
                        try
                        {
                            var client = MakeClient();
                            webClients.Add(client);
                            OperationsRequested++;
                            responseSerialized = await client.UploadStringTaskAsync(
                                "https://pubsub.googleapis.com/v1/projects/" + _projectName + "/subscriptions/" +
                                topicSubscription + ":pull",
                                "POST",
                                requestSerialized);
                        }
                        catch (WebException ex)
                        {
                            if (ex.Status == WebExceptionStatus.KeepAliveFailure)
                            {
                                // This is expected for long polling.
                                responseSerialized = "{}";
                            }
                            else
                            {
                                var exx = ex.Response as HttpWebResponse;
                                if (exx.StatusCode == HttpStatusCode.BadGateway || exx.StatusCode == HttpStatusCode.GatewayTimeout)
                                {
                                    // Just let the long poll happen again.
                                    responseSerialized = "{}";
                                }
                                throw;
                            }
                        }

                        Debug.WriteLine("POLL TASK RESPONSE RECEIVED");

                        var response = JsonConvert.DeserializeObject<dynamic>(responseSerialized);
                        if (response.receivedMessages != null)
                        {
                            foreach (var msg in response.receivedMessages)
                            {
                                messages.Add(new Message(MakeClient(),
                                    "https://pubsub.googleapis.com/v1/projects/" + _projectName + "/subscriptions/" +
                                    topicSubscription + ":acknowledge", (string)msg.ackId)
                                {
                                    Data = msg.message.data,
                                    Attributes = msg.message.attributes,
                                });
                            }
                        }
                    }
                    finally
                    {
                        Debug.WriteLine("POLL TASK FINISHED OPERATION");
                    }
                }, cancellationToken)).ToArray();

            if (immediate)
            {
                Task.WaitAll(tasks);
            }
            else
            {
                Task.WaitAny(tasks);
                foreach (var client in webClients)
                {
                    client.CancelAsync();
                }
                cancellationTokenSource.Cancel();
                try
                {
                    Task.WaitAll(tasks);
                }
                catch (Exception)
                {
                    // ignored
                }
            }
            
            Debug.WriteLine("POLL FINISHED");

            if (immediate && tasks.Any(x => x.Status == TaskStatus.Faulted))
            {
                throw new AggregateException(tasks.Where(x => x.Exception != null).SelectMany(x => x.Exception.InnerExceptions));
            }

            if (!immediate && tasks.Count(x => x.Status == TaskStatus.Faulted) == tasks.Length)
            {
                throw new AggregateException(tasks.Where(x => x.Exception != null).SelectMany(x => x.Exception.InnerExceptions));
            }

            return messages.ToList();
        }

        /// <summary>
        /// Publishes a message to the given Pub/Sub topic.
        /// </summary>
        /// <param name="topic">The Pub/Sub topic name.</param>
        /// <param name="data">The string data to set as the message.  If this is <c>null</c>, then no data is attached.</param>
        /// <param name="attributes">The message attributes as string-string pairs.  If this is <c>null</c>, then no attributes are attached.</param>
        public void Publish(string topic, string data, Dictionary<string, string> attributes)
        {
            CheckToken();

            var message = new Dictionary<string, object>();
            if (data != null)
            {
                message["data"] = data;
            }
            if (attributes != null)
            {
                message["attributes"] = attributes;
            }
            if (data == null && attributes == null)
            {
                throw new InvalidOperationException("You must specify data or attributes (or both).");
            }

            var request = new
            {
                messages = new[]
                {
                    message
                }
            };
            var requestSerialized = JsonConvert.SerializeObject(request);
            MakeRetryableRequest(
                "https://pubsub.googleapis.com/v1/projects/" + _projectName + "/topics/" + topic + ":publish",
                "POST",
                requestSerialized);
        }

        /// <summary>
        /// Internally makes a retryable request to the given URL, with the given method
        /// and upload data.
        /// <para>
        /// As with all cloud services, Google Cloud servers may be unresponsive or timeout
        /// when requests are made to them.  This method retries the request if a timeout
        /// occurs, or if the OAuth token has expired by the time the request was made (in
        /// which case it requests a new token).
        /// </para>
        /// </summary>
        /// <param name="url">The URL to send the HTTP request to.</param>
        /// <param name="method">The method to use.</param>
        /// <param name="data">The data to upload.</param>
        private void MakeRetryableRequest(string url, string method, string data)
        {
            var succeeded = false;
            while (!succeeded)
            {
                try
                {
                    CheckToken();
                    OperationsRequested++;
                    MakeClient().UploadString(url, method, data);
                    succeeded = true;
                }
                catch (WebException ex)
                {
                    var httpWebResponse = ex.Response as HttpWebResponse;
                    if (httpWebResponse != null)
                    {
                        if (httpWebResponse.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            // Retry with CheckToken.
                            Thread.Sleep(1000);
                            continue;
                        }
                    }
                    if (ex.Status == WebExceptionStatus.Timeout)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }
                    throw;
                }
            }
        }

        /// <summary>
        /// Unsubscribes from the given Pub/Sub topic.  The arguments passed to this
        /// function should be the same as those passed to <see cref="Subscribe"/>.
        /// </summary>
        /// <param name="topic">The topic name.</param>
        /// <param name="subscription">The subscription name.</param>
        public void Unsubscribe(string topic, string subscription)
        {
            CheckToken();
            
            MakeRetryableRequest(
                "https://pubsub.googleapis.com/v1/projects/" + _projectName + "/subscriptions/t-" + topic + "-s-" + subscription,
                "DELETE",
                string.Empty);
        }

        public void ClearQueue()
        {
            CheckToken();

            Debug.WriteLine("CLEAR QUEUE START");

            if (_subscriptions.Count == 0)
            {
                throw new InvalidOperationException("You can't poll when you're not subscribed to anything!");
            }

            var webClients = new ConcurrentBag<WebClient>();
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            var tasks = (from topicSubscription in _subscriptions
                         select Task.Run(async () =>
                         {
                             var didAck = 10000;
                             while (didAck > 0)
                             {
                                 didAck = 0;

                                 var request = new
                                 {
                                     maxMessages = 10000,
                                     returnImmediately = true,
                                 };
                                 var requestSerialized = JsonConvert.SerializeObject(request);
                                 try
                                 {
                                     Debug.WriteLine("CLEAR QUEUE TASK STARTED OPERATION");

                                     string responseSerialized;
                                     try
                                     {
                                         using (var client = MakeClient())
                                         {
                                             webClients.Add(client);
                                             OperationsRequested++;
                                             responseSerialized = await client.UploadStringTaskAsync(
                                                 "https://pubsub.googleapis.com/v1/projects/" + _projectName + "/subscriptions/" +
                                                 topicSubscription + ":pull",
                                                 "POST",
                                                 requestSerialized);
                                         }
                                     }
                                     catch (WebException ex)
                                     {
                                         if (ex.Status == WebExceptionStatus.KeepAliveFailure)
                                         {
                                             // This is expected for long polling.
                                             responseSerialized = "{}";
                                         }
                                         else
                                         {
                                             var exx = ex.Response as HttpWebResponse;
                                             if (exx.StatusCode == HttpStatusCode.BadGateway || exx.StatusCode == HttpStatusCode.GatewayTimeout)
                                             {
                                                 // Just let the long poll happen again.
                                                 responseSerialized = "{}";
                                             }
                                             throw;
                                         }
                                     }

                                     Debug.WriteLine("CLEAR QUEUE TASK RESPONSE RECEIVED");

                                     var toAck = new List<string>();

                                     var response = JsonConvert.DeserializeObject<dynamic>(responseSerialized);
                                     if (response.receivedMessages != null)
                                     {
                                         foreach (var msg in response.receivedMessages)
                                         {
                                             toAck.Add((string)msg.ackId);
                                         }
                                     }

                                     Debug.WriteLine("To clear: " + toAck.Count);

                                     var ackRequest = new
                                     {
                                         ackIds = toAck.ToArray()
                                     };
                                     var ackRequestSerialized = JsonConvert.SerializeObject(ackRequest);
                                     using (var client2 = MakeClient())
                                     {
                                         client2.UploadString(
                                              "https://pubsub.googleapis.com/v1/projects/" + _projectName + "/subscriptions/" +
                                              topicSubscription + ":acknowledge",
                                             "POST",
                                             ackRequestSerialized);
                                     }

                                     Debug.WriteLine("Acked: " + toAck.Count);

                                     didAck = toAck.Count;
                                 }
                                 finally
                                 {
                                     Debug.WriteLine("CLEAR QUEUE TASK FINISHED OPERATION");
                                 }
                             }
                         }, cancellationToken)).ToArray();

            Task.WaitAll(tasks);

            Debug.WriteLine("CLEAR QUEUE FINISHED");

            if (tasks.Any(x => x.Status == TaskStatus.Faulted))
            {
                throw new AggregateException(tasks.Where(x => x.Exception != null).SelectMany(x => x.Exception.InnerExceptions));
            }
        }

        /// <summary>
        /// Deletes the given Pub/Sub topic.  The arguments passed to this
        /// function should be the same as those passed to <see cref="CreateTopic"/>.
        /// </summary>
        /// <param name="topic">The topic name.</param>
        public void DeleteTopic(string topic)
        {
            CheckToken();

            MakeRetryableRequest(
                "https://pubsub.googleapis.com/v1/projects/" + _projectName + "/topics/" + topic,
                "DELETE",
                string.Empty);
        }
    }
}