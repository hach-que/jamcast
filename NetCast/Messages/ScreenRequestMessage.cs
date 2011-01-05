using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Runtime.Serialization;

namespace NetCast.Messages
{
    [Serializable()]
    public class ScreenRequestMessage : Message, ISerializable
    {
        public ScreenRequestMessage(IPEndPoint self)
            : base(self)
        {
        }

        public ScreenRequestMessage(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
