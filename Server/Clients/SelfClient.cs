using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace JamCast.Clients
{
    public class SelfClient : Client
    {
        private Bitmap m_Bitmap = null;

        public SelfClient()
        {
        }

        public override Bitmap Screen
        {
            get { return this.m_Bitmap; }
        }

        public override string Name
        {
            get { return "Local Server"; }
        }

        public override void DisposeBitmaps(bool isActive)
        {
        }

        public override void Refresh()
        {
            // Use the GDI call to create a DC to the whole display.
            IntPtr dc1 = Gdi.CreateDisplay();
            Graphics g1 = Graphics.FromHdc(dc1);

            // Get the bitmap to draw on, creating it if necessary.
            if (this.m_Bitmap == null || this.m_Bitmap.Width != System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width ||
                this.m_Bitmap.Height != System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height)
            {
                if (this.m_Bitmap != null)
                    this.m_Bitmap.Dispose();

                this.m_Bitmap = new Bitmap(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height, g1);
            }
            Graphics g2 = Graphics.FromImage(this.m_Bitmap);
            g2.Clear(Color.Black);

            // Now reacquire the device context for both the bitmap and the screen
            // Apparently you have to do this and can't go directly from the original device context
            // or exceptions are thrown when you attempt to release the device contexts.
            dc1 = g1.GetHdc();
            IntPtr dc2 = g2.GetHdc();

            // Bit blast the screen onto the bitmap.
            Gdi.BitBlt(dc2, 0, 0, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height, dc1, 0, 0, 13369376);

            // Release the device contexts.
            g1.ReleaseHdc(dc1);
            g2.ReleaseHdc(dc2);

            // Draw the mouse cursor.
            Cursors.Arrow.Draw(g2, new Rectangle(
                new Point(
                    Cursor.Position.X - Cursors.Arrow.HotSpot.X,
                    Cursor.Position.Y - Cursors.Arrow.HotSpot.Y
                    ),
                new Size(32, 32)
                ));
        }
    }
}
