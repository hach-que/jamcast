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
        public void Send(IPEndPoint endpoint)
        {
            // Send the message to the target.
            UdpClient udp = new UdpClient();
            using (MemoryStream writer = new MemoryStream())
            {
                //this.Dht.Log(Dht.LogType.DEBUG, "Sending -");
                //this.Dht.Log(Dht.LogType.DEBUG, "          Message - " + this.ToString());
                //this.Dht.Log(Dht.LogType.DEBUG, "          Target - " + target.ToString());
                Message.p_Formatter.Serialize(writer, this);
                int bytes = udp.Send(writer.GetBuffer(), writer.GetBuffer().Length, endpoint);
                //this.Dht.Log(Dht.LogType.DEBUG, bytes + " total bytes sent.");
            }
            udp.Close();
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
