using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace JamCast
{
    public abstract class Client
    {
        /// <summary>
        /// Retrieves a bitmap of the client's screen.
        /// </summary>
        /// <returns>A bitmap of the client's screen.</returns>
        public abstract Bitmap GetScreen();        
    }
}
