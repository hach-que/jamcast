using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO.Compression;

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

            try
            {
                // Create writing stream.
                MemoryStream writer = new MemoryStream();

                try
                {
                    // Serialize.
                    Message.p_Formatter.Serialize(writer, this);

                    // Create other streams.
                    MemoryStream reader = new MemoryStream();
                    GZipStream compress = new GZipStream(reader, CompressionMode.Compress);

                    try
                    {
                        // Compress the data.
                        writer.Seek(0, SeekOrigin.Begin);
                        byte[] buffer = new byte[4096]; int read;
                        while ((read = writer.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            compress.Write(buffer, 0, read);
                        }
                        compress.Close();

                        // Send the UDP packet.
                        udp.Send(reader.GetBuffer(), reader.GetBuffer().Length, endpoint);
                    }
                    catch (Exception)
                    {
                        // Silence errors.
                    }
                    finally
                    {
                        compress.Close();
                        compress.Dispose();
                        reader.Close();
                        reader.Dispose();
                    }
                }
                catch (Exception)
                {
                    // Silence errors.
                }
                finally
                {
                    writer.Close();
                    writer.Dispose();
                }
            }
            catch (Exception)
            {
                // Silence errors.
            }
            finally
            {
                udp.Close();
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
            try
            {
                tcp.Connect(endpoint);

                // Create writing stream.
                MemoryStream writer = new MemoryStream();

                try
                {
                    // Serialize.
                    Message.p_Formatter.Serialize(writer, this);

                    // Create the other streams.
                    MemoryStream reader = new MemoryStream();
                    GZipStream compress = new GZipStream(reader, CompressionMode.Compress);

                    try
                    {
                        // Compress the data.
                        writer.Seek(0, SeekOrigin.Begin);
                        byte[] buffer = new byte[4096]; int read;
                        while ((read = writer.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            compress.Write(buffer, 0, read);
                        }
                        compress.Close();

                        // Send length information.
                        byte[] len = System.BitConverter.GetBytes(reader.GetBuffer().Length);
                        if (len.Length != 4)
                            throw new ApplicationException("Integer is not 4 bytes on this PC!");
                        tcp.Client.Send(len, SocketFlags.None);

                        // Send TCP packet.
                        tcp.Client.Send(reader.GetBuffer(), 0, reader.GetBuffer().Length, SocketFlags.None);
                    }
                    catch (Exception)
                    {
                        // Silence errors.
                    }
                    finally
                    {
                        compress.Close();
                        compress.Dispose();
                        reader.Close();
                        reader.Dispose();
                    }
                }
                catch (Exception)
                {
                    // Silence errors.
                }
                finally
                {
                    writer.Close();
                    writer.Dispose();
                }
            }
            catch (Exception)
            {
                // Silence errors.
            }
            finally
            {
                tcp.Close();
            }
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
