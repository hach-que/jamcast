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
        private IPEndPoint p_UdpEndPoint = null;
        private IPEndPoint p_TcpEndPoint = null;
        private TcpListener m_TcpListener = null;
        private UdpClient m_UdpClient = null;
        private Thread m_TcpThread = null;
        private Thread m_UdpThread = null;
        //private Dictionary<IPAddress, Dictionary<int, byte[]>> m_PacketReconstructor = new Dictionary<IPAddress, Dictionary<int, byte[]>>();

        public event EventHandler<MessageEventArgs> OnReceived;

        public Queue(int udpport, int tcpport)
        {
            // Automatically get the internal IP address (since JamCast is designed for
            // LAN networks).
            string hostname = Dns.GetHostName();
            this.p_UdpEndPoint = new IPEndPoint(Dns.GetHostByName(hostname).AddressList[0], udpport);
            this.p_TcpEndPoint = new IPEndPoint(Dns.GetHostByName(hostname).AddressList[0], tcpport);

            // Start listening for events on UDP.
            this.m_UdpClient = new UdpClient(this.p_UdpEndPoint.Port);
            this.m_UdpThread = new Thread(delegate()
            {
                try
                {
                    while (true)
                    {
                        IPEndPoint from = null;
                        byte[] result = this.m_UdpClient.Receive(ref from);
                        this.OnReceive(from, result, result.Length);
                        //this.Log(LogType.DEBUG, "Received a message from " + from.ToString());
                    }
                }
                catch (Exception e)
                {
                    if (e is ThreadAbortException)
                        return;
                    Console.WriteLine(e.ToString());
                    throw e;
                }
            }
            );
            this.m_UdpThread.IsBackground = true;
            this.m_UdpThread.Start();

            // Start listening for events on TCP.
            this.m_TcpListener = new TcpListener(this.p_TcpEndPoint.Port);
            this.m_TcpThread = new Thread(delegate()
            {
                try
                {
                    this.m_TcpListener.Start();
                    while (true)
                    {
                        TcpClient client = this.m_TcpListener.AcceptTcpClient();
                        new Thread(() =>
                            {
                                try
                                {
                                    // Read the length header.
                                    byte[] lenbytes = new byte[4];
                                    int lbytesread = client.Client.Receive(lenbytes, 0, 4, SocketFlags.None);
                                    if (lbytesread != 4) return; // drop this packet :(
                                    int length = System.BitConverter.ToInt32(lenbytes, 0);
                                    int r = 0;

                                    // Read the actual data.
                                    byte[] result = new byte[length];
                                    while (r < length)
                                    {
                                        int bytes = client.Client.Receive(result, r, length - r, SocketFlags.None);
                                        r += bytes;
                                    }

                                    this.OnReceive(client.Client.RemoteEndPoint as IPEndPoint, result, length);
                                }
                                catch (SocketException)
                                {
                                    // Do nothing.
                                }
                            }).Start();
                        //this.Log(LogType.DEBUG, "Received a message from " + from.ToString());
                    }
                }
                catch (Exception e)
                {
                    if (e is ThreadAbortException)
                        return;
                    Console.WriteLine(e.ToString());
                    throw e;
                }
            }
            );
            this.m_TcpThread.IsBackground = true;
            this.m_TcpThread.Start();
        }

        /// <summary>
        /// Handles receiving data through the UdpClient.
        /// </summary>
        /// <param name="endpoint">The endpoint from which the message was received.</param>
        /// <param name="result">The data that was received.</param>
        private void OnReceive(IPEndPoint endpoint, byte[] result, int length)
        {
            // Do an OnReceive event.
            using (MemoryStream stream = new MemoryStream(result, 0, length))
            {
                Message message = Queue.p_Formatter.Deserialize(stream) as Message;
                MessageEventArgs e = new MessageEventArgs(message);

                if (this.OnReceived != null)
                    this.OnReceived(this, e);
            }

            /*

            // Read ordering information.
            short i = Convert.ToInt16(result[0]), t = Convert.ToInt16(result[1]);

            // Check to see if there's a dictionary for this endpoint.
            if (!this.m_PacketReconstructor.Keys.Contains(endpoint.Address))
                this.m_PacketReconstructor.Add(endpoint.Address, new Dictionary<int,byte[]>());

            // Add the data.
            byte[] data = new byte[result.Length - 2];
            for (int c = 2; c < result.Length; c += 1)
                data[c - 2] = result[c];
            this.m_PacketReconstructor[endpoint.Address].Add(i, data);

            // Check to see if we have all the data.
            if (this.HasAll(endpoint.Address, t))
            {
                // We have all the packets, reconstruct.
                List<byte> full = new List<byte>();
                for (int c = 0; c < t; c += 1)
                {
                    foreach (byte b in this.m_PacketReconstructor[endpoint.Address][c])
                        full.Add(b);
                }

                this.m_PacketReconstructor.Remove(endpoint.Address);

                // Do an OnReceive event.
                using (MemoryStream stream = new MemoryStream(full.ToArray()))
                {
                    Message message = Queue.p_Formatter.Deserialize(stream) as Message;
                    MessageEventArgs e = new MessageEventArgs(message);

                    if (this.OnReceived != null)
                        this.OnReceived(this, e);
                }
            }
        }

        /// <summary>
        /// Whether all of the packets have arrived from a particular endpoint.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="total"></param>
        /// <returns></returns>
        private bool HasAll(IPAddress address, short total)
        {
            Dictionary<int, bool> count = new Dictionary<int, bool>();
            foreach (KeyValuePair<int, byte[]> kv in this.m_PacketReconstructor[address])
                count.Add(kv.Key, true);
            for (int i = 0; i < total; i += 1)
            {
                if (!count.Keys.Contains(i) || count[i] != true)
                    return false;
            }
            return true;*/
        }

        /// <summary>
        /// The internal, LAN-based UDP endpoint of this computer.
        /// </summary>
        public IPEndPoint UdpSelf
        {
            get
            {
                return this.p_UdpEndPoint;
            }
        }

        /// <summary>
        /// The internal, LAN-based TCP endpoint of this computer.
        /// </summary>
        public IPEndPoint TcpSelf
        {
            get
            {
                return this.p_TcpEndPoint;
            }
        }
    }
}
