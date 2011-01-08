using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public abstract Bitmap GetScreen();
    }
}
