using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace NetCast
{
    [Serializable()]
    public abstract class Message : ISerializable
    {
        protected static readonly IFormatter p_Formatter = new BinaryFormatter();
        private IPEndPoint p_Source = null;

        public Message(IPEndPoint self)
        {
            this.p_Source = self;
        }

        public Message(SerializationInfo info, StreamingContext context)
        {
            this.p_Source = info.GetValue("message.source", typeof(IPEndPoint)) as IPEndPoint;
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("message.source", this.p_Source, typeof(IPEndPoint));
        }

        /// <summary>
        /// Sends the message to the specified endpoint.
        /// </summary>
        /// <param name="endpoint"></param>
        public void SendUDP(IPEndPoint endpoint)
        {
            // Send UDP instead.  Message must be smaller than UDP packet size.
            UdpClient udp = new UdpClient();
            using (MemoryStream writer = new MemoryStream())
            {
                Message.p_Formatter.Serialize(writer, this);
                udp.Send(writer.GetBuffer(), writer.GetBuffer().Length, endpoint);
            }
            return;
        }

        /// <summary>
        /// Sends the message to the specified endpoint.
        /// </summary>
        /// <param name="endpoint"></param>
        public void SendTCP(IPEndPoint endpoint)
        {
            // Send the message to the target.
            TcpClient tcp = new TcpClient();
            tcp.Connect(endpoint);
            using (MemoryStream writer = new MemoryStream())
            {
                Message.p_Formatter.Serialize(writer, this);
                byte[] len = System.BitConverter.GetBytes(writer.GetBuffer().Length);
                if (len.Length != 4)
                    throw new ApplicationException("Integer is not 4 bytes on this PC!");
                tcp.Client.Send(len, SocketFlags.None);
                tcp.Client.Send(writer.GetBuffer(), 0, writer.GetBuffer().Length, SocketFlags.None);
            }
            tcp.Close();
        }

        public IPEndPoint Source
        {
            get
            {
                return this.p_Source;
            }
            set
            {
                this.p_Source = value;
            }
        }
    }
}
