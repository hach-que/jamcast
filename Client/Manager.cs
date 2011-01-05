using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NetCast.Messages;
using NetCast;
using System.Net;

namespace Client
{
    public class Manager
    {
        private NetCast.Queue p_NetCast = null;

        /// <summary>
        /// Starts the manager cycle.
        /// </summary>
        public void Run()
        {
            // Start the NetCast listener.
            this.p_NetCast = new NetCast.Queue(12000);
            this.p_NetCast.OnReceived += new EventHandler<MessageEventArgs>(p_NetCast_OnReceived);

            // Advertise client service to the server.
            ClientServiceStartingMessage cssm = new ClientServiceStartingMessage(this.p_NetCast.Self);
            cssm.Send(new IPEndPoint(IPAddress.Broadcast, 12001));

            // Start the main application loop.
            Application.Run();
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
                MessageBox.Show("Countdown: " + (e.Message as CountdownBroadcastMessage).SecondsRemaining);
            }
        }
    }
}
