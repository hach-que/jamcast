using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Drawing;
using NetCast;
using NetCast.Messages;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Threading;
using System.Collections.Concurrent;

namespace JamCast.Clients
{
    public class NetworkClient : Client
    {
        private Bitmap m_RealBitmap = null;
        private Bitmap p_CachedBitmap = null;
        private object m_BitmapLocker = new object();
        private Queue m_Queue = null;
        private IPEndPoint p_Source = null;
        private string p_CachedName = "Unknown!";
        private bool m_Waiting = false;

        public NetworkClient(Queue queue, IPEndPoint source, string name)
        {
            // Set variables.
            this.m_Queue = queue;
            this.p_Source = source;
            this.p_CachedName = name;

            // Register OnReceived handler.
            this.m_Queue.OnReceived += new EventHandler<MessageEventArgs>(m_Queue_OnReceived);
        }

        ~NetworkClient()
        {
            BitmapTracker.Purge();
        }

        public override Bitmap Screen
        {
            get { return this.p_CachedBitmap; }
        }

        public override string Name
        {
            get { return this.p_CachedName; }
        }

        /// <summary>
        /// This event is raised when a message is received.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void m_Queue_OnReceived(object sender, MessageEventArgs e)
        {
            if (e.Message is ScreenResultMessage && e.Message.Source.Address.Equals(this.p_Source.Address))
            {
                ScreenResultMessage srm = e.Message as ScreenResultMessage;
                lock (this.m_BitmapLocker)
                {
                    BitmapTracker.Tag(this.m_RealBitmap);
                    this.m_RealBitmap = srm.Bitmap;
                    this.m_Waiting = false;
                }
            }
        }

        public override void Refresh()
        {
            if (!this.m_Waiting)
            {
                this.m_Waiting = true;

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
                lock (this.m_BitmapLocker)
                {
                    if (this.m_RealBitmap == null)
                    {
                        Bitmap b = new Bitmap(640, 360);
                        Graphics g = Graphics.FromImage(b);
                        g.Clear(Color.Black);
                        g.DrawString("Waiting for screen from " + this.p_Source.ToString(), SystemFonts.CaptionFont, Brushes.White, new PointF(24, 24));
                        BitmapTracker.Tag(b);
                        this.p_CachedBitmap = b;
                    }
                    else
                    {
                        BitmapTracker.Tag(this.m_RealBitmap);
                        this.p_CachedBitmap = this.m_RealBitmap;
                    }
                }
            }
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
