using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NetCast.Messages;
using NetCast;
using System.Net;
using System.Drawing;

namespace Client
{
    public class Manager
    {
        private NetCast.Queue p_NetCast = null;
        private Bitmap m_Bitmap = null;

        /// <summary>
        /// Starts the manager cycle.
        /// </summary>
        public void Run()
        {
            // Start the NetCast listener.
            this.p_NetCast = new NetCast.Queue(12000);
            this.p_NetCast.OnReceived += new EventHandler<MessageEventArgs>(p_NetCast_OnReceived);

            // Advertise client service to the server.
            ClientServiceStartingMessage cssm = new ClientServiceStartingMessage(this.p_NetCast.Self);
            cssm.Send(new IPEndPoint(IPAddress.Broadcast, 12001));

            // Start the main application loop.
            Application.Run();
        }

        /// <summary>
        /// This event is fired when a network message has been received.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void p_NetCast_OnReceived(object sender, MessageEventArgs e)
        {
            if (e.Message is CountdownBroadcastMessage)
            {
                MessageBox.Show("Countdown: " + (e.Message as CountdownBroadcastMessage).SecondsRemaining);
            }
            else if (e.Message is ScreenRequestMessage)
            {
                ScreenResultMessage srm = new ScreenResultMessage(this.p_NetCast.Self, this.GetScreen());
                srm.Send(e.Message.Source);
            }
        }

        public Bitmap GetScreen()
        {
            // Use the GDI call to create a DC to the whole display.
            IntPtr dc1 = Gdi.CreateDisplay();
            Graphics g1 = Graphics.FromHdc(dc1);

            // Get the bitmap to draw on, creating it if necessary.
            if (this.m_Bitmap == null || this.m_Bitmap.Width != Screen.PrimaryScreen.Bounds.Width ||
                this.m_Bitmap.Height != Screen.PrimaryScreen.Bounds.Height)
            {
                if (this.m_Bitmap != null)
                    this.m_Bitmap.Dispose();

                this.m_Bitmap = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, g1);
            }
            Graphics g2 = Graphics.FromImage(this.m_Bitmap);
            g2.Clear(Color.Black);

            // Now reacquire the device context for both the bitmap and the screen
            // Apparently you have to do this and can't go directly from the original device context
            // or exceptions are thrown when you attempt to release the device contexts.
            dc1 = g1.GetHdc();
            IntPtr dc2 = g2.GetHdc();

            // Bit blast the screen onto the bitmap.
            Gdi.BitBlt(dc2, 0, 0, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, dc1, 0, 0, 13369376);

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

            // Return the image.
            return this.m_Bitmap;
        }
    }
}
