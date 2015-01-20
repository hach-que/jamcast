using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using NetCast.Messages;

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
        private bool p_Running = true;
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
            this.m_UdpThread = new Thread(this.HandleUdpClient);
            this.m_UdpThread.IsBackground = true;
            this.m_UdpThread.Start();

            // Start listening for events on TCP.
            this.m_TcpListener = new TcpListener(this.p_TcpEndPoint.Port);
            this.m_TcpThread = new Thread(this.HandleTcpListener);
            this.m_TcpThread.IsBackground = true;
            this.m_TcpThread.Start();
        }

        /// <summary>
        /// Handles messages arriving from the UDP client.
        /// </summary>
        private void HandleUdpClient()
        {
            try
            {
                while (this.p_Running)
                {
                    IPEndPoint from = null;
                    byte[] result = this.m_UdpClient.Receive(ref from);
                    if (result.Length != 0)
                    {
                        this.OnReceive(from, result.ToList(), result.Length);
                        Console.WriteLine("Received UDP packet from " + from.Address.ToString() + ".");
                    }
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

        /// <summary>
        /// Handles the TCP listener accept loop.
        /// </summary>
        private void HandleTcpListener()
        {
            try
            {
                this.m_TcpListener.Start();
                while (this.p_Running)
                {
                    TcpClient client = this.m_TcpListener.AcceptTcpClient();
                    Thread tt = new Thread(this.HandleTcpClient);
                    tt.IsBackground = true;
                    tt.Start(client as object);
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

        /// <summary>
        /// Handles the TCP client on the other end of the connection.
        /// </summary>
        /// <param name="o">The TCP client object.</param>
        private void HandleTcpClient(object o)
        {
            TcpClient client = o as TcpClient;
            IPEndPoint endpoint = null;
            List<byte> data = new List<byte>();
            try
            {
                // Copy endpoint information.
                endpoint = new IPEndPoint((client.Client.RemoteEndPoint as IPEndPoint).Address, (client.Client.RemoteEndPoint as IPEndPoint).Port);

                // Read the length header.
                byte[] lenbytes = new byte[4];
                int lbytesread = client.Client.Receive(lenbytes, 0, 4, SocketFlags.None);
                if (lbytesread != 4) return; // drop this packet :(
                int length = BitConverter.ToInt32(lenbytes, 0);
                int r = 0;

                // Read the actual data.
                byte[] result = new byte[length];
                while (r < length)
                {
                    int bytes = client.Client.Receive(result, r, length - r, SocketFlags.None);
                    r += bytes;
                }

                // Copy the data.
                data.AddRange(result);
            }
            catch (SocketException e)
            {
                // Socket exception raised, force disconnect.
                this.ForceDisconnect(endpoint);
            }
            finally
            {
                client.Close();
            }

            // Perform the OnReceive event.
            Console.WriteLine("Received TCP packet from " + endpoint.Address.ToString() + ".");
            this.OnReceive(endpoint, data, data.Count);
        }

        /// <summary>
        /// Simulates a correct client disconnection, used when the TCP socket fails or serialization fails.
        /// </summary>
        /// <param name="endpoint">The endpoint of the client.</param>
        private void ForceDisconnect(IPEndPoint endpoint)
        {
            MessageEventArgs m = new MessageEventArgs(new ClientServiceStoppingMessage(endpoint));
            m.Message.Source.Address = endpoint.Address;

            if (this.OnReceived != null)
                this.OnReceived(this, m);
        }

        /// <summary>
        /// Handles receiving data through the UdpClient.
        /// </summary>
        /// <param name="endpoint">The endpoint from which the message was received.</param>
        /// <param name="result">The data that was received.</param>
        private void OnReceive(IPEndPoint endpoint, List<byte> result, int length)
        {
            // Create the streams.
            MemoryStream stream = new MemoryStream(result.ToArray());
            GZipStream decompress = new GZipStream(stream, CompressionMode.Decompress, true);

            Message message = null;
            try
            {
                // Decompress.
                message = p_Formatter.Deserialize(decompress) as Message;
            }
            catch (SerializationException e)
            {
                // Corruption in the stream, force disconnect.
                this.ForceDisconnect(endpoint);
            }
            finally
            {
                // Close the streams.
                stream.Close();
                stream.Dispose();
                decompress.Close();
                decompress.Dispose();
            }

            // Free memory.
            result.Clear();

            // Do the OnReceive call.
            if (message != null)
            {
                MessageEventArgs e = new MessageEventArgs(message);
                e.Message.Source.Address = endpoint.Address;

                if (this.OnReceived != null)
                    this.OnReceived(this, e);
            }
        }

        /// <summary>
        /// Shuts down the queue, forcing the TCP and UDP listeners and threads to close immediately.
        /// </summary>
        public void Stop()
        {
            this.p_Running = false;
            this.m_TcpThread.Abort();
            this.m_UdpThread.Abort();
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
