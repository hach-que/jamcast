using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Runtime.Serialization;
using System.IO;

namespace NetCast.Messages
{
    [Serializable()]
    public class ClientServiceStartingMessage : Message, ISerializable
    {
        private string p_Name = "Unknown!";

        public ClientServiceStartingMessage(IPEndPoint endpoint, string name) : base(endpoint)
        {
            this.p_Name = name;
        }

        public ClientServiceStartingMessage(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                this.p_Name = info.GetString("user.name");
            }
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            using (MemoryStream stream = new MemoryStream())
            {
                info.AddValue("user.name", this.p_Name);
            }
        }

        public string Name
        {
            get { return this.p_Name; }
        }
    }
}
