using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace NetCast
{
    public class Queue
    {
        protected static readonly IFormatter p_Formatter = new BinaryFormatter();
        private IPEndPoint p_EndPoint = null;
        private UdpClient m_UdpClient = null;
        private Thread m_UdpThread = null;

        public event EventHandler<MessageEventArgs> OnReceived;

        public Queue(int port)
        {
            // Automatically get the internal IP address (since JamCast is designed for
            // LAN networks).
            string hostname = Dns.GetHostName();
            this.p_EndPoint = new IPEndPoint(Dns.GetHostByName(hostname).AddressList[0], port);

            // Start listening for events.
            IPEndPoint from = null;
            this.m_UdpClient = new UdpClient(this.p_EndPoint.Port);
            this.m_UdpThread = new Thread(delegate()
            {
                try
                {
                    while (true)
                    {
                        byte[] result = this.m_UdpClient.Receive(ref from);
                        //this.Log(LogType.DEBUG, "Received a message from " + from.ToString());
                        this.OnReceive(from, result);
                    }
                }
                catch (Exception e)
                {
                    if (e is ThreadAbortException)
                        return;
                    Console.WriteLine(e.ToString());
                }
            }
            );
            //this.m_UdpThread.IsBackground = true;
            this.m_UdpThread.Start();
        }

        /// <summary>
        /// Handles receiving data through the UdpClient.
        /// </summary>
        /// <param name="endpoint">The endpoint from which the message was received.</param>
        /// <param name="result">The data that was received.</param>
        private void OnReceive(IPEndPoint endpoint, byte[] result)
        {
            using (MemoryStream stream = new MemoryStream(result))
            {
                Message message = Queue.p_Formatter.Deserialize(stream) as Message;
                MessageEventArgs e = new MessageEventArgs(message);
                // We trust that the message has the correct endpoint.
                //message.Source = endpoint;

                if (this.OnReceived != null)
                    this.OnReceived(this, e);
            }
        }

        /// <summary>
        /// The internal, LAN-based IP address of this computer.
        /// </summary>
        public IPEndPoint Self
        {
            get
            {
                return this.p_EndPoint;
            }
        }
    }
}
