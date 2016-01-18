using System;
using System.Net;
using System.Threading;
using NetCast;
using NetCast.Messages;

namespace Client
{
    public partial class Manager
    {
        private Queue _netCast = null;
        private string _name = "Unknown!";
        private string _email = string.Empty;

        /// <summary>
        /// Starts the manager cycle.
        /// </summary>
        public void Run()
        {
			LoadUsername();

			ListenForApplicationExit(OnStop);

            // Start the NetCast listener.
            this._netCast = new Queue(12000, 12001);
            this._netCast.OnReceived += new EventHandler<MessageEventArgs>(p_NetCast_OnReceived);

			ConfigureSystemTrayIcon();

            // Advertise client service to the server.
            var t = new Thread(() =>
                {
                    while (true)
                    {
                        var message = new ClientServiceStartingMessage(this._netCast.TcpSelf, this._name);
                        message.SendUDP(new IPEndPoint(IPAddress.Broadcast, 13000));
                        Thread.Sleep(1000);
                    }
                });
            t.IsBackground = true;
            t.Start();
        }

		private void OnStop()
        {
            var message = new ClientServiceStoppingMessage(this._netCast.TcpSelf);
            message.SendUDP(new IPEndPoint(IPAddress.Broadcast, 13000));
            Thread.Sleep(1000);
            this._netCast.Stop();
        }

        /// <summary>
        /// This event is fired when a network message has been received.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void p_NetCast_OnReceived(object sender, MessageEventArgs e)
        {
            if (e.Message is CountdownBroadcastMessage)
            {
				SetTrayIconToCountdown();
            }
            else if (e.Message is BeginStreamingMessage)
            {
				SetTrayIconToOn();

                string sdp;
                StartStreaming(e.Message.Source.Address, out sdp, () =>
                {
                    SetTrayIconToOff();

                    StopStreaming(e.Message.Source.Address);

                    var stoppedMessage = new StreamingStoppedMessage(this._netCast.TcpSelf);
                    stoppedMessage.SendTCP(e.Message.Source);
                });

                var startedMessage = new StreamingStartedMessage(this._netCast.TcpSelf, sdp);
                startedMessage.SendTCP(e.Message.Source);
            }
            else if (e.Message is EndStreamingMessage)
            {
                SetTrayIconToOff();

                StopStreaming(e.Message.Source.Address);

                var stoppedMessage = new StreamingStoppedMessage(this._netCast.TcpSelf);
                stoppedMessage.SendTCP(e.Message.Source);
            }
        }

        public Queue NetCast
        {
            get { return this._netCast; }
        }

        public string User { get { return this._name; } }
    }
}
