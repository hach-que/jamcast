using System;
using System.Net;
using System.Runtime.Serialization;

namespace NetCast.Messages
{
    [Serializable()]
    public class ClientServiceStoppingMessage : Message, ISerializable
    {
        public ClientServiceStoppingMessage(IPEndPoint endpoint) : base(endpoint)
        {
        }

        public ClientServiceStoppingMessage(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
