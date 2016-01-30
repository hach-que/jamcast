using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Projector.Controllers;

namespace Projector
{
    public class ClientSelectionController : IController
    {
        private ClientListEntry _selectedClient;

        private ClientListController _clientListController;

        private DateTime? _lastSwitch;

        private Random _random = new Random();

        public ClientListEntry SelectedClient
        {
            get
            {
                return this._selectedClient;
            }
        }

        public void ClientStoppedStream(string clientGuid)
        {
            var client = this._clientListController.GetClients().FirstOrDefault(x => x.Guid == clientGuid);
            if (this._selectedClient == client)
            {
                this._selectedClient = PickAnotherClient();
            }
        }

        public void Update(PubSubController pubSubController, TwitterRetrieveController twitterRetrieveController, TwitterProcessingController twitterProcessingController, ClientListController clientListController, ClientSelectionController clientSelectionController, StreamController streamController, FfmpegProcessController ffmpegProcessController)
        {
            _clientListController = clientListController;

            if (this._selectedClient == null)
            {
                //Debug.WriteLine("Currently selected client: (no-one)");
            }
            else
            {
                //Debug.WriteLine("Currently selected client: " + this._selectedClient.Guid);
            }

            if (this._lastSwitch == null || this._lastSwitch.Value < DateTime.UtcNow.AddMinutes(-1) || this._selectedClient == null)
            {
                this._lastSwitch = DateTime.UtcNow;
                this._selectedClient = this.PickAnotherClient();
            }
        }

        private ClientListEntry PickAnotherClient()
        {
            var clients = this._clientListController.GetClients().ToArray();
            if (clients.Length == 0)
            {
                return null;
            }

            if (this._selectedClient != null && clients.Length > 1)
            {
                var newSet = clients.Where(x => x != this._selectedClient).ToArray();
                return newSet[_random.Next(0, newSet.Length)];
            }
            else
            {
                return clients[_random.Next(0, clients.Length)];
            }
        }

        public void ForceNewClient()
        {
            this._selectedClient = this.PickAnotherClient();
        }
    }
}
