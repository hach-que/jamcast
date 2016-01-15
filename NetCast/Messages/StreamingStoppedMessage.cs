using System;
using System.Net;
using System.Runtime.Serialization;

namespace NetCast.Messages
{
    [Serializable]
    public class StreamingStoppedMessage : Message, ISerializable
    {
        public StreamingStoppedMessage(IPEndPoint self)
            : base(self)
        {
        }

        public StreamingStoppedMessage(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}