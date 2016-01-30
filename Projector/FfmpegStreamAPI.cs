using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace Projector
{
    public class FfmpegStreamAPI
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll")]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        public Process PlayTo(Form form, string sdp)
        {
            var ffplay = Path.Combine(new FileInfo(Assembly.GetEntryAssembly().Location).DirectoryName, "ffplay.exe");
            var processStartInfo = new ProcessStartInfo(ffplay, "-i -");
            processStartInfo.RedirectStandardInput = true;
            processStartInfo.UseShellExecute = false;
            processStartInfo.CreateNoWindow = true;
            var process = Process.Start(processStartInfo);
            process.EnableRaisingEvents = true;

            process.StandardInput.Write(sdp);
            process.StandardInput.Close();

            while (process.MainWindowHandle == IntPtr.Zero)
            {
                Thread.Sleep(50);
            }

            form.Invoke(new Action(() =>
            {
                SetParent(process.MainWindowHandle, form.Handle);
                //Thread.Sleep(100);
                //RemoveBorder(process.MainWindowHandle);
                //Thread.Sleep(100);
                //MoveWindow(process.MainWindowHandle, 0, 0, 320, 320, false);
            }));

            return process;
        }

        public static void AlignToFormBounds(Process process, Form form, Rectangle rectangle)
        {
            RemoveBorder(process.MainWindowHandle, form.Handle);
            //SetParent(process.MainWindowHandle, form.Handle);
            MoveWindow(process.MainWindowHandle, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, false);
            //form.Focus();
        }

        [DllImport("USER32.DLL")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        //Gets window attributes
        [DllImport("USER32.DLL")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        private static void RemoveBorder(IntPtr windowHandle, IntPtr parentHandle)
        {
            const uint WS_BORDER = 0x00800000;
            const uint WS_DLGFRAME = 0x00400000;
            const uint WS_THICKFRAME = 0x00040000;
            const uint WS_CAPTION = WS_BORDER | WS_DLGFRAME;
            const uint WS_MINIMIZE = 0x20000000;
            const uint WS_MAXIMIZE = 0x01000000;
            const uint WS_SYSMENU = 0x00080000;
            const uint WS_VISIBLE = 0x10000000;
            const int GWL_STYLE = -16;

            var currentstyle = (uint)GetWindowLong(windowHandle, GWL_STYLE);
            currentstyle &= ~WS_CAPTION;
            currentstyle &= ~WS_SYSMENU;
            currentstyle &= ~WS_THICKFRAME;
            currentstyle &= ~WS_MINIMIZE;
            currentstyle &= ~WS_MAXIMIZE;
            SetWindowLong(windowHandle, GWL_STYLE, (int)currentstyle);
            SetWindowLong(windowHandle, -8, parentHandle.ToInt32());/*

            uint[] styles = new uint[] { WS_CAPTION, WS_THICKFRAME, WS_MINIMIZE, WS_MAXIMIZE, WS_SYSMENU };

            foreach (uint style in styles)
            {

                if ((currentstyle & style) != 0)
                {

                    if (removeBorder)
                    {

                        currentstyle &= ~style;
                    }
                    else
                    {

                        currentstyle |= style;
                    }
                }
            }

            SetWindowLong(windowHandle, GWL_STYLE, (IntPtr)(currentstyle));
            //this resizes the window to the client area and back. Also forces the window to redraw.
            if (removeBorder)
            {

                SetWindowLong(windowHandle, (IntPtr)0, this.PointToScreen(this.ClientRectangle.Location).X, this.PointToScreen(this.ClientRectangle.Location).Y, this.ClientRectangle.Width, this.ClientRectangle.Height, 0);
            }
            else
            {

                Set*WindowLong(windowHandle, (IntPtr)0, originallocation.X, originallocation.Y, originalsize.Width, originalsize.Height, 0);
            }*/
        }
    }
}
