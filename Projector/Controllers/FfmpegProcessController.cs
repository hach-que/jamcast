using System.Diagnostics;
using System.Windows.Forms;

using JamCast;

namespace Projector.Controllers
{
    public class FfmpegProcessController : IController
    {
        private readonly Form _broadcastForm;

        private ClientSelectionController _clientSelectionController;

        private Process _ffplayProcess;

        private ClientListEntry _streamingStartedClient;

        private StreamController _streamController;

        public FfmpegProcessController(Form broadcastForm)
        {
            this._broadcastForm = broadcastForm;
        }

        public Process FfplayProcess
        {
            get
            {
                return this._ffplayProcess;
            }
        }

        public void StreamingActuallyStarted(string clientGuid, string sdp)
        {
            if (this._clientSelectionController.SelectedClient == null)
            {
                return;
            }

            if (clientGuid == this._clientSelectionController.SelectedClient.Guid)
            {
                this._streamingStartedClient = this._clientSelectionController.SelectedClient;

                var ffplay = new FfmpegStreamAPI();
                this._ffplayProcess = ffplay.PlayTo(_broadcastForm, sdp);
                this._ffplayProcess.Exited += (o, args) =>
                    {
                        this._streamingStartedClient = null;
                    _clientSelectionController.ForceNewClient();
                };
                if (this._ffplayProcess.HasExited)
                {
                    this._streamingStartedClient = null;
                    _clientSelectionController.ForceNewClient();
                }
            }
        }

        public void Update(PubSubController pubSubController, TwitterRetrieveController twitterRetrieveController, TwitterProcessingController twitterProcessingController, ClientListController clientListController, ClientSelectionController clientSelectionController, StreamController streamController, FfmpegProcessController ffmpegProcessController)
        {
            _clientSelectionController = clientSelectionController;
            _streamController = streamController;

            if (this._streamingStartedClient == null)
            {
                //Debug.WriteLine("Currently streaming from: (no-one)");
            }
            else
            {
                //Debug.WriteLine("Currently streaming from: " + this._streamingStartedClient.Guid);
            }

            if (this._ffplayProcess == null || this._ffplayProcess.HasExited)
            {
                //Debug.WriteLine("ffplay is not running");
            }
            else
            {
               // Debug.WriteLine("ffplay is running");
            }

            if (streamController.StreamingClient != this._streamingStartedClient)
            {
                this._streamingStartedClient = null;
                this._ffplayProcess?.Kill();
                this._ffplayProcess = null;
            }
        }

        public string WaitingOn
        {
            get
            {
                if (_streamController == null)
                {
                    return null;
                }

                if (_streamController.StreamingClient != null &&
                    this._streamingStartedClient == null)
                {
                    return _streamController.StreamingClient.UserFullName;
                }

                return null;
            }
        }
    }
}