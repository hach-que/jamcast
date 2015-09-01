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
            Thread t = new Thread(() =>
                {
                    while (true)
                    {
                        ClientServiceStartingMessage cssm = new ClientServiceStartingMessage(this.p_NetCast.TcpSelf, this.m_Name);
                        cssm.SendUDP(new IPEndPoint(IPAddress.Broadcast, 13000));
                        Thread.Sleep(1000);
                    }
                });
            t.IsBackground = true;
            t.Start();
        }

		private void OnStop()
        {
            ClientServiceStoppingMessage cssm = new ClientServiceStoppingMessage(this.p_NetCast.TcpSelf);
            cssm.SendUDP(new IPEndPoint(IPAddress.Broadcast, 13000));
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
            else if (e.Message is ScreenRequestMessage)
            {
				SetTrayIconToOn();

                // Send the screen result.
                ScreenResultMessage srm = new ScreenResultMessage(this.p_NetCast.TcpSelf, this.GetScreen());
                srm.SendTCP(e.Message.Source);

				ScheduleTrayIconToOff();
            }
        }

        public Queue NetCast
        {
            get { return this.p_NetCast; }
        }

        public string User { get { return this.m_Name; } }
    }
}
