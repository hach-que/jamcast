using System.Collections.Generic;
using System.Diagnostics;

using Projector.Controllers;

namespace Projector
{
    public class ClientListController : IController
    {
        private List<ClientListEntry> _clients;

        public IReadOnlyCollection<ClientListEntry> GetClients()
        {
            if (this._clients == null)
            {
                return new List<ClientListEntry>().AsReadOnly();
            }

            return _clients.AsReadOnly();
        }

        public void Update(PubSubController pubSubController, TwitterRetrieveController twitterRetrieveController, TwitterProcessingController twitterProcessingController, ClientListController clientListController, ClientSelectionController clientSelectionController, StreamController streamController, FfmpegProcessController ffmpegProcessController)
        {
            //Debug.WriteLine("Currently knows " + GetClients().Count + " clients");
        }

        public void UpdateClientsFromNewList(List<ClientListEntry> clients)
        {
            this._clients = new List<ClientListEntry>(clients);
        }
    }
}