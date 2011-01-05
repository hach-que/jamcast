using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetCast
{
    public class MessageEventArgs : EventArgs
    {
        private Message p_Message = null;

        public MessageEventArgs(Message message)
        {
            this.p_Message = message;
        }

        public Message Message
        {
            get { return this.p_Message; }
        }
    }
}
