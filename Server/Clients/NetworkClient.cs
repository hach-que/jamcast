using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Drawing;
using NetCast;
using NetCast.Messages;
using System.Net.Sockets;

namespace JamCast.Clients
{
    public class NetworkClient : Client
    {
        private Bitmap m_Bitmap = null;
        private Queue m_Queue = null;
        private IPEndPoint p_Source = null;

        public NetworkClient(Queue queue, IPEndPoint source)
        {
            // Set variables.
            this.m_Queue = queue;
            this.p_Source = source;

            // Register OnReceived handler.
            this.m_Queue.OnReceived += new EventHandler<MessageEventArgs>(m_Queue_OnReceived);
        }

        /// <summary>
        /// This event is raised when a message is received.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void m_Queue_OnReceived(object sender, MessageEventArgs e)
        {
            if (e.Message is ScreenResultMessage)
            {
                ScreenResultMessage srm = e.Message as ScreenResultMessage;
                this.m_Bitmap = srm.Bitmap.Clone() as Bitmap;
            }
        }

        public override System.Drawing.Bitmap GetScreen()
        {
            // Send a request for a screen bitmap.
            try
            {
                ScreenRequestMessage srm = new ScreenRequestMessage(this.m_Queue.TcpSelf);
                srm.SendTCP(this.p_Source);
            }
            catch (SocketException)
            {
                // The client disconnected.
                this.Disconnect(this, new EventArgs());
            }

            // Check to see if we have bitmap data.  Since the above operation is asynchronous,
            // it's likely we won't for the first few frame requests due to network delay.
            if (this.m_Bitmap == null)
            {
                Bitmap b = new Bitmap(480, 480);
                Graphics g = Graphics.FromImage(b);
                g.Clear(Color.Red);
                g.DrawString("WAITING FOR SCREEN FROM " + this.p_Source.ToString(), SystemFonts.CaptionFont, Brushes.White, new PointF(24, 24));
                return b;
            }
            else
                return this.m_Bitmap;
        }

        public IPEndPoint Source
        {
            get
            {
                return this.p_Source;
            }
        }
    }
}
