using System;
using System.Net;
using System.Threading;
using NetCast;
using NetCast.Messages;

namespace Client
{
    public partial class Manager
    {
        private Queue p_NetCast = null;
        private string m_Name = "Unknown!";

        /// <summary>
        /// Starts the manager cycle.
        /// </summary>
        public void Run()
        {
			LoadUsername();

			ListenForApplicationExit(OnStop);

            // Start the NetCast listener.
            this.p_NetCast = new Queue(12000, 12001);
            this.p_NetCast.OnReceived += new EventHandler<MessageEventArgs>(p_NetCast_OnReceived);

			ConfigureSystemTrayIcon();

            // Advertise client service to the server.
            var t = new Thread(() =>
                {
                    while (true)
                    {
                        var message = new ClientServiceStartingMessage(this.p_NetCast.TcpSelf, this.m_Name);
                        message.SendUDP(new IPEndPoint(IPAddress.Broadcast, 13000));
                        Thread.Sleep(1000);
                    }
                });
            t.IsBackground = true;
            t.Start();
        }

		private void OnStop()
        {
            var message = new ClientServiceStoppingMessage(this.p_NetCast.TcpSelf);
            message.SendUDP(new IPEndPoint(IPAddress.Broadcast, 13000));
            Thread.Sleep(1000);
            this.p_NetCast.Stop();
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

                    var stoppedMessage = new StreamingStoppedMessage(this.p_NetCast.TcpSelf);
                    stoppedMessage.SendTCP(e.Message.Source);
                });

                var startedMessage = new StreamingStartedMessage(this.p_NetCast.TcpSelf, sdp);
                startedMessage.SendTCP(e.Message.Source);
            }
            else if (e.Message is EndStreamingMessage)
            {
                SetTrayIconToOff();

                StopStreaming(e.Message.Source.Address);

                var stoppedMessage = new StreamingStoppedMessage(this.p_NetCast.TcpSelf);
                stoppedMessage.SendTCP(e.Message.Source);
            }
        }

        public Queue NetCast
        {
            get { return this.p_NetCast; }
        }

        public string User { get { return this.m_Name; } }
    }
}
