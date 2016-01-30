using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Projector.Controllers;

namespace Projector
{
    public class StreamController : IController
    {
        public ClientListEntry StreamingClient { get; private set; }

        public void Update(PubSubController pubSubController, TwitterRetrieveController twitterRetrieveController, TwitterProcessingController twitterProcessingController, ClientListController clientListController, ClientSelectionController clientSelectionController, StreamController streamController, FfmpegProcessController ffmpegProcessController)
        {
            var selectedClient = clientSelectionController.SelectedClient;

            if (selectedClient == null)
            {
                return;
            }

            if (this.StreamingClient != selectedClient)
            {
                //Debug.WriteLine("Streaming client is no longer matching selected client");

                if (this.StreamingClient != null)
                {
                    // Stop the old stream.
                    //Debug.WriteLine("Sending request to stop old stream from: " + StreamingClient.Guid);
                    pubSubController.RequestStopStreaming(StreamingClient);
                }

                // Send out a start streaming request to the client.
                //Debug.WriteLine("Sending request to start new stream from: " + selectedClient.Guid);
                pubSubController.RequestStartStreaming(selectedClient);
                StreamingClient = selectedClient;
            }
        }
    }
}
