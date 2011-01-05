using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Drawing;
using NetCast;

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
        }

        public override System.Drawing.Bitmap GetScreen()
        {
            Bitmap b = new Bitmap(480, 480);
            Graphics g = Graphics.FromImage(b);
            g.Clear(Color.Red);
            g.DrawString("NO SERVICE FROM " + this.p_Source.ToString(), SystemFonts.CaptionFont, Brushes.White, new PointF(24, 24));
            return b;
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
