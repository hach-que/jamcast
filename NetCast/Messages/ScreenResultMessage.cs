using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Runtime.Serialization;
using System.Drawing;

namespace NetCast.Messages
{
    [Serializable()]
    public class ScreenResultMessage : Message, ISerializable
    {
        private Bitmap p_Bitmap = null;

        public ScreenResultMessage(IPEndPoint self, Bitmap data)
            : base(self)
        {
            this.p_Bitmap = data;
        }

        public ScreenResultMessage(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.p_Bitmap = info.GetValue("data.bitmap", typeof(Bitmap)) as Bitmap;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("data.bitmap", this.p_Bitmap, typeof(Bitmap));
        }

        public Bitmap Bitmap
        {
            get
            {
                return this.p_Bitmap;
            }
        }
    }
}
