using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Client
{
    public static class Gdi
    {
        [DllImport("gdi32.dll")]
        public static extern bool BitBlt(
            IntPtr hdcDest, // handle to destination DC
            int nXDest,  // x-coord of destination upper-left corner
            int nYDest,  // y-coord of destination upper-left corner
            int nWidth,  // width of destination rectangle
            int nHeight, // height of destination rectangle
            IntPtr hdcSrc,  // handle to source DC
            int nXSrc,   // x-coordinate of source upper-left corner
            int nYSrc,   // y-coordinate of source upper-left corner
            System.Int32 dwRop  // raster operation code
            );

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateDC(
            string lpszDriver,
            string lpszDevice,
            string lpszOutput,
            string lpInitData
            );

        public static IntPtr CreateDisplay()
        {
            return Gdi.CreateDC("DISPLAY", null, null, null);
        }
    }
}
