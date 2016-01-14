﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GooglePubSub
{
    public class Message
    {
        private readonly WebClient _client;
        private readonly string _subscriptionUrl;
        private readonly string _ackId;

        internal Message(WebClient client, string subscriptionUrl, string ackId)
        {
            _client = client;
            _subscriptionUrl = subscriptionUrl;
            _ackId = ackId;
        }

        public string Data { get; set; }
        public Dictionary<string, string> Attributes { get; set; }

        public void Acknowledge()
        {
            var request = new
            {
                ackIds = new[]
                {
                    _ackId
                }
            };
            var requestSerialized = JsonConvert.SerializeObject(request);
            var responseSerialized = _client.UploadString(
                _subscriptionUrl,
                "POST",
                requestSerialized);
        }
    }
}
