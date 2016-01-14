#if PLATFORM_WINDOWS

using System;
using System.Runtime.InteropServices;

namespace Client
{
    public static class Gdi
    {
        [DllImport("gdi32.dll")]
        /// <summary>The BitBlt function performs a bit-block transfer of the color data corresponding to a rectangle of pixels from the specified source device context into a destination device context.</summary>
        /// <param name="hdcDest">handle to destination DC</param>
        /// <param name="nXDest">x-coord of destination upper-left corner</param>
        /// <param name="nYDest">y-coord of destination upper-left corner</param>
        /// <param name="nWidth">width of destination rectangle</param>
        /// <param name="nHeight">height of destination rectangle</param>
        /// <param name="hdcSrc">handle to source DC</param>
        /// <param name="nXSrc">x-coordinate of source upper-left corner</param>
        /// <param name="nYSrc">y-coordinate of source upper-left corner</param>
        /// <param name="dwRop">raster operation code</param>
        /// <see cref="https://msdn.microsoft.com/en-us/library/windows/desktop/dd183370%28v=vs.85%29.aspx"/>
        public static extern bool BitBlt(
            IntPtr hdcDest, // handle to destination DC
            int nXDest,  // x-coord of destination upper-left corner
            int nYDest,  // y-coord of destination upper-left corner
            int nWidth,  // width of destination rectangle
            int nHeight, // height of destination rectangle
            IntPtr hdcSrc,  // handle to source DC
            int nXSrc,   // x-coordinate of source upper-left corner
            int nYSrc,   // y-coordinate of source upper-left corner
            Int32 dwRop  // raster operation code
            );

        [DllImport("gdi32.dll")]
        /// <summary>The CreateDC function creates a device context (DC) for a device using the specified name.</summary>
        /// <param name="lpszDriver">A pointer to a null-terminated character string that specifies either DISPLAY or the name of a specific display device. For printing, we recommend that you pass NULL to lpszDriver because GDI ignores lpszDriver for printer devices.</param>
        /// <param name="lpszDevice">A pointer to a null-terminated character string that specifies the name of the specific output device being used, as shown by the Print Manager (for example, Epson FX-80). It is not the printer model name. The lpszDevice parameter must be used.</param>
        /// <param param name="lpszOutput">This parameter is ignored and should be set to NULL.It is provided only for compatibility with 16-bit Windows.</param>
        /// <param name="lpInitData">A pointer to a DEVMODE structure containing device-specific initialization data for the device driver. The DocumentProperties function retrieves this structure filled in for a specified device. The lpInitData parameter must be NULL if the device driver is to use the default initialization (if any) specified by the user.</param>
        private static extern IntPtr CreateDC(
            string lpszDriver,
            string lpszDevice,
            string lpszOutput,
            string lpInitData
            );

        /// <summary>
        /// Creates the Device Context for the Display, and returns a pointer to it
        /// </summary>
        /// <returns></returns>
        public static IntPtr CreateDisplay()
        {
            return CreateDC("DISPLAY", null, null, null);
        }
    }
}

#endif