using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace NetCast.Messages
{
    [Serializable()]
    public class CountdownBroadcastMessage : Message, ISerializable
    {
        private int p_SecondsRemaining = 10;

        public CountdownBroadcastMessage(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.p_SecondsRemaining = info.GetInt32("data.seconds");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("data.seconds", this.p_SecondsRemaining, typeof(int));
        }

        public int SecondsRemaining
        {
            get
            {
                return this.p_SecondsRemaining;
            }
        }
    }
}
