using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using NetCast;
using NetCast.Messages;
using Projector;

namespace JamCast
{
    public class Client
    {
        private Queue m_Queue = null;
        private IPEndPoint p_Source = null;
        private string p_CachedName = "Unknown!";
        private Form _broadcastForm;

        public Client(Queue queue, Form broadcastForm, IPEndPoint source, string name)
        {
            // Set variables.
            this.m_Queue = queue;
            _broadcastForm = broadcastForm;
            this.p_Source = source;
            this.p_CachedName = name;

            // Register OnReceived handler.
            this.m_Queue.OnReceived += new EventHandler<MessageEventArgs>(m_Queue_OnReceived);
        }

        public event EventHandler OnDisconnected;
        protected void Disconnect(object sender, EventArgs e)
        {
            if (this.OnDisconnected != null)
                this.OnDisconnected(sender, e);
        }

        ~Client()
        {
            BitmapTracker.Purge();
        }

        public string Name
        {
            get { return this.p_CachedName; }
        }

        public Process FfplayProcess { get; set; }

        /// <summary>
        /// This event is raised when a message is received.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void m_Queue_OnReceived(object sender, MessageEventArgs e)
        {
            if (e.Message is StreamingStartedMessage && e.Message.Source.Address.Equals(this.p_Source.Address))
            {
                var srm = e.Message as StreamingStartedMessage;

                if (FfplayProcess != null && !FfplayProcess.HasExited)
                {
                    FfplayProcess.Kill();
                    FfplayProcess = null;
                }

                var ffplay = new FfplayStreamController();
                FfplayProcess = ffplay.PlayTo(_broadcastForm, srm.SdpInfo);
                FfplayProcess.Exited += (o, args) =>
                {
                    var end = new EndStreamingMessage(this.m_Queue.TcpSelf);
                    end.SendTCP(this.p_Source);
                };
                if (FfplayProcess.HasExited)
                {
                    var end = new EndStreamingMessage(this.m_Queue.TcpSelf);
                    end.SendTCP(this.p_Source);
                }
            }
        }

        public void Start()
        {
            try
            {
                var srm = new BeginStreamingMessage(this.m_Queue.TcpSelf);
                srm.SendTCP(this.p_Source);
            }
            catch (SocketException)
            {
                // The client disconnected.
                this.Disconnect(this, new EventArgs());
            }
        }

        public void Stop()
        {
            try
            {
                var srm = new EndStreamingMessage(this.m_Queue.TcpSelf);
                srm.SendTCP(this.p_Source);

                FfplayProcess.Kill();
                FfplayProcess = null;
            }
            catch (SocketException)
            {
                // The client disconnected.
                this.Disconnect(this, new EventArgs());
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
