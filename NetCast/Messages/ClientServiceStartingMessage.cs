using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Runtime.Serialization;

namespace NetCast.Messages
{
    [Serializable()]
    public class ClientServiceStartingMessage : Message, ISerializable
    {
        public ClientServiceStartingMessage(IPEndPoint endpoint) : base(endpoint)
        {
        }

        public ClientServiceStartingMessage(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
