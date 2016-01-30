using System;
using System.Collections.Generic;
using System.IO;

using GooglePubSub;

using Projector.Controllers;

namespace Projector
{
    public class PubSubController : IController
    {
        private PubSubPersistent _pubSub;

        private string _guid;

        private ClientListController _clientListController;

        private ClientSelectionController _clientSelectionController;

        private FfmpegProcessController _ffmpegProcessController;

        private void Init()
        {
            using (var reader = new StreamReader(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "JamCast", "guid.txt")))
            {
                _guid = reader.ReadToEnd().Trim();
            }

            this._pubSub = new PubSubPersistent();
            this._pubSub.TopicsToCreate.Add("projectors");
            this._pubSub.SubscriptionsToCreate.Add(new KeyValuePair<string, string>("projectors", "projector-" + _guid));
            this._pubSub.Reconfigure(
                "http://melbggj16.info/jamcast/gettoken/api",
                "melbourne-global-game-jam-16");
            this._pubSub.MessageRecieved += PubSubOnMessageRecieved;
        }

        public PubSubPersistentStatus Status
        {
            get
            {
                return this._pubSub.Status;
            }
        }

        private void PubSubOnMessageRecieved(object sender, MessageReceivedEventHandlerArgs args)
        {
            var message = args.Message;

            switch ((string)message.Type)
            {
                case "update-clients":
                    {
                        var clients = new List<ClientListEntry>();
                        foreach (var client in message.Clients)
                        {
                            clients.Add(new ClientListEntry
                            {
                                Guid = (string)client.Guid,
                                IPv4Address = (string)client.IPv4Address,
                                UserFullName = (string)client.UserFullName
                            });
                        }
                        this._clientListController.UpdateClientsFromNewList(clients);
                    }
                    break;
                case "stop-streaming":
                    {
                        this._clientSelectionController.ClientStoppedStream((string)message.ClientGuid);
                        break;
                    }
                case "started-streaming":
                    {
                        this._ffmpegProcessController.StreamingActuallyStarted((string)message.ClientGuid, (string)message.Sdp);
                        break;
                    }
            }
        }

        public void Update(PubSubController pubSubController, TwitterRetrieveController twitterRetrieveController, TwitterProcessingController twitterProcessingController, ClientListController clientListController, ClientSelectionController clientSelectionController, StreamController streamController, FfmpegProcessController ffmpegProcessController)
        {
            _clientListController = clientListController;
            _clientSelectionController = clientSelectionController;
            _ffmpegProcessController = ffmpegProcessController;

            if (this._pubSub == null)
            {
                Init();
            }
        }

        public void RequestStartStreaming(ClientListEntry client)
        {
            this._pubSub.Publish("client-" + client.Guid, new {
                Type = "start",
                DesiredIPAddress = client.IPv4Address,
            });
        }

        public void RequestStopStreaming(ClientListEntry client)
        {
            this._pubSub.Publish("client-" + client.Guid, new
            {
                Type = "stop",
                DesiredIPAddress = client.IPv4Address,
            });
        }
    }
}