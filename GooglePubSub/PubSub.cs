using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;

namespace GooglePubSub
{
    public class PubSub
    {
        private readonly string _token;
        private readonly string _projectName;
        private readonly WebClient _client;
        private readonly List<string> _subscriptions;

        /// <summary>
        /// Creates a new node in a many-to-many PubSub system.
        /// </summary>
        /// <param name="projectName">
        /// A project URL like "melbourne-global-game-jam-16"
        /// </param>
        /// <param name="token">
        /// The API token to use.
        /// </param>
        public PubSub(string projectName, string token)
        {
            _projectName = projectName;
            _token = token;
            _client = new WebClient();
            _subscriptions = new List<string>();
        }

        public void CreateTopic(string topic)
        {
            var request = new
            {
                name = "projects/" + _projectName + "/topics/" + topic
            };
            var requestSerialized = JsonConvert.SerializeObject(request);
            var responseSerialized = _client.UploadString(
                "https://pubsub.googleapis.com/v1/" + request.name + "?key=" +
                _token,
                "PUT",
                requestSerialized);
        }

        public void Subscribe(string topic, string subscription)
        {
            var request = new
            {
                name = "projects/" + _projectName + "/subscriptions/" + subscription,
                topic = "projects/" + _projectName + "/topics/" + topic
            };
            var requestSerialized = JsonConvert.SerializeObject(request);
            var responseSerialized = _client.UploadString(
                "https://pubsub.googleapis.com/v1/projects/" + _projectName + "/subscriptions/" + subscription + "?key=" +
                _token,
                "PUT",
                requestSerialized);
            if (!_subscriptions.Contains(request.name))
            {
                _subscriptions.Add(request.name);
            }
        }

        public List<Message> Poll(int messagesPerSubscription)
        {
            var messages = new List<Message>();

            foreach (var subscription in _subscriptions)
            {
                var request = new
                {
                    name = "projects/" + _projectName + "/subscriptions/" + subscription,
                    maxMessages = messagesPerSubscription,
                    returnImmediately = true,
                };
                var requestSerialized = JsonConvert.SerializeObject(request);
                var responseSerialized = _client.UploadString(
                    "https://pubsub.googleapis.com/v1/projects/" + _projectName + "/subscriptions/" + subscription + ":pull?key=" +
                    _token,
                    "POST",
                    requestSerialized);

                try
                {
                    var response = JsonConvert.DeserializeObject<dynamic>(responseSerialized);
                    foreach (var msg in response.receivedMessages)
                    {
                        messages.Add(new Message(_client, "https://pubsub.googleapis.com/v1/projects/" + _projectName + "/subscriptions/" + subscription + ":acknowledge?key=" + _token, msg.ackId)
                        {
                            Data = msg.message.data,
                            Attributes = msg.message.attributes,
                        });
                    }
                }
                catch (Exception)
                {
                }
            }

            return messages;
        }

        public void Publish(string topic, string data, Dictionary<string, string> attributes)
        {
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
            var responseSerialized = _client.UploadString(
                "https://pubsub.googleapis.com/v1/projects/" + _projectName + "/topics/" + topic + ":publish?key=" +
                _token,
                "POST",
                requestSerialized);
        }

        public List<Message> LongPoll(string subscription, int messagesPerSubscription)
        {
            var messages = new List<Message>();
            
            var request = new
            {
                name = "projects/" + _projectName + "/subscriptions/" + subscription,
                maxMessages = messagesPerSubscription,
            };
            var requestSerialized = JsonConvert.SerializeObject(request);
            var responseSerialized = _client.UploadString(
                "https://pubsub.googleapis.com/v1/projects/" + _projectName + "/subscriptions/" + subscription + ":pull?key=" +
                _token,
                "POST",
                requestSerialized);

            try
            {
                var response = JsonConvert.DeserializeObject<dynamic>(responseSerialized);
                foreach (var msg in response.receivedMessages)
                {
                    messages.Add(new Message(_client, "https://pubsub.googleapis.com/v1/projects/" + _projectName + "/subscriptions/" + subscription + ":acknowledge?key=" + _token, msg.ackId)
                    {
                        Data = msg.message.data,
                    });
                }
            }
            catch (Exception)
            {
            }

            return messages;
        }

        public void UnsubscribeFromAllTopics(string subscription)
        {
            _client.UploadString(
                "https://pubsub.googleapis.com/v1/projects/" + _projectName + "/subscriptions/" + subscription + "?key=" +
                _token,
                "DELETE",
                string.Empty);
        }

        public void DeleteTopic(string topic)
        {
            _client.UploadString(
                "https://pubsub.googleapis.com/v1/projects/" + _projectName + "/topics/" + topic + "?key=" +
                _token,
                "DELETE",
                string.Empty);
        }
    }
}