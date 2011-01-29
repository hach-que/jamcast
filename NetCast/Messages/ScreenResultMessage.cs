﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Runtime.Serialization;
using System.Drawing;
using System.IO;

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
            using (MemoryStream stream = new MemoryStream())
            {
                byte[] data = info.GetValue("data.bitmap", typeof(byte[])) as byte[];
                stream.Write(data, 0, data.Length);
                this.p_Bitmap = new Bitmap(stream);
            }
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            using (MemoryStream stream = new MemoryStream())
            {
                this.p_Bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                info.AddValue("data.bitmap", stream.GetBuffer(), typeof(byte[]));
            }
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