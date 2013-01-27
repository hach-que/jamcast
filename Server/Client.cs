﻿using System;
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

        /// <summary>
        /// Disposes the bitmaps that need to be freed.  This should
        /// be called on the main UI thread WHEN SWITCHING CLIENTS
        /// so that bitmaps are not disposed while still being used.
        /// </summary>
        public abstract void DisposeBitmaps(bool isActive);
    }
}
