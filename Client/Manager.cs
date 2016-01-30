using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

using GooglePubSub;
namespace Client
{
    public partial class Manager
    {
        private PubSubPersistent _pubSub;
        private string _name = "Unknown!";
        private string _email = string.Empty;

        private string _guid;

        /// <summary>
        /// Starts the manager cycle.
        /// </summary>
        public void Run()
        {
			LoadUsername();

			ListenForApplicationExit(OnStop);
            
            using (var reader = new StreamReader(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "JamCast", "guid.txt")))
            {
                _guid = reader.ReadToEnd().Trim();
            }

            _pubSub = new PubSubPersistent();
            this._pubSub.TopicsToCreate.Add("projectors");
            this._pubSub.TopicsToCreate.Add("client-" + _guid);
            this._pubSub.SubscriptionsToCreate.Add(new KeyValuePair<string, string>(
                "client-" + _guid,
                "client-" + _guid));
            this._pubSub.Reconfigure(
                "http://melbggj16.info/jamcast/gettoken/api",
                "melbourne-global-game-jam-16");
            this._pubSub.MessageRecieved += PubSubOnMessageRecieved;

			ConfigureSystemTrayIcon();
        }

        private void PubSubOnMessageRecieved(object sender, MessageReceivedEventHandlerArgs args)
        {
            var message = args.Message;

            switch ((string)message.Type)
            {
                case "countdown":
                    this.SetTrayIconToCountdown();
                    break;
                case "start":
                    {
                        SetTrayIconToOn();

                        var address = IPAddress.Parse((string)message.DesiredIPAddress);

                        string sdp;
                        StartStreaming(address, out sdp, () =>
                        {
                            SetTrayIconToOff();

                            StopStreaming(address);

                            this._pubSub.Publish("projectors", new { Type = "stop-streaming", ClientGuid = _guid });
                        });

                        this._pubSub.Publish("projectors", new { Type = "started-streaming", ClientGuid = _guid, Sdp = sdp });
                    }
                    break;
                case "stop":
                    {
                        SetTrayIconToOff();

                        var address = IPAddress.Parse((string)message.DesiredIPAddress);

                        StopStreaming(address);

                        this._pubSub.Publish("projectors", new { Type = "stop-streaming", ClientGuid = _guid });
                    }
                    break;
            }
        }

        private void OnStop()
        {
            this._pubSub.Stop();
        }

        public string User { get { return this._name; } }
    }
}
