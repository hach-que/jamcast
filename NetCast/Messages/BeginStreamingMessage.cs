using System;
using System.Net;
using System.Runtime.Serialization;

namespace NetCast.Messages
{
    [Serializable()]
    public class BeginStreamingMessage : Message, ISerializable
    {
        public BeginStreamingMessage(IPEndPoint self)
            : base(self)
        {
        }

        public BeginStreamingMessage(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
