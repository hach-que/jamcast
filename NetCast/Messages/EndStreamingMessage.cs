using System;
using System.Net;
using System.Runtime.Serialization;

namespace NetCast.Messages
{
    [Serializable()]
    public class EndStreamingMessage : Message, ISerializable
    {
        public EndStreamingMessage(IPEndPoint self)
            : base(self)
        {
        }

        public EndStreamingMessage(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
