using System;

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
