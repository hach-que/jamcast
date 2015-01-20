using System;
using System.Drawing;

namespace JamCast
{
    public abstract class Client
    {
        public event EventHandler OnDisconnected;
        protected void Disconnect(object sender, EventArgs e)
        {
            if (this.OnDisconnected != null)
                this.OnDisconnected(sender, e);
        }

        /// <summary>
        /// Retrieves a bitmap of the client's screen.
        /// </summary>
        /// <returns>A bitmap of the client's screen.</returns>
        public abstract Bitmap Screen
        {
            get;
        }

        /// <summary>
        /// Retrieves the name of a client.
        /// </summary>
        /// <returns>The client's name.</returns>
        public abstract string Name
        {
            get;
        }

        /// <summary>
        /// Refreshs the cached image of the client's screen.
        /// </summary>
        public abstract void Refresh();
    }
}
