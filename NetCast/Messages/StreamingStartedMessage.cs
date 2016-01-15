using System;
using System.Net;
using System.Runtime.Serialization;

namespace NetCast.Messages
{
    [Serializable]
    public class StreamingStartedMessage : Message, ISerializable
    {
        public StreamingStartedMessage(IPEndPoint self, string sdp)
            : base(self)
        {
            SdpInfo = sdp;
        }

        public StreamingStartedMessage(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            SdpInfo = info.GetValue("data.sdpinfo", typeof (string)) as string;
        }

        public string SdpInfo { get; set; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("data.sdpinfo", SdpInfo);
        }
    }
}