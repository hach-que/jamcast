using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NetCast.Messages;
using NetCast;
using System.Net;
using System.Drawing;
using System.Threading;
using Client.Properties;

namespace Client
{
    public class Manager
    {
        private NetCast.Queue p_NetCast = null;
        //private Bitmap m_Bitmap = null;
        private TrayIcon m_TrayIcon = null;
        private System.Windows.Forms.Timer m_OffTimer = new System.Windows.Forms.Timer();
        private string m_Name = "Unknown!";

        /// <summary>
        /// Starts the manager cycle.
        /// </summary>
        public void Run()
        {
            // Ask the person who they are ;)
            WhoAreYou way = new WhoAreYou();
            if (way.ShowDialog() != DialogResult.OK)
            {
                Application.Exit();
                return;
            }
            this.m_Name = way.Name;
            way.Dispose();
            GC.Collect();

            // Listen for the application exit event.
            Application.ApplicationExit += new EventHandler(Application_ApplicationExit);

            // Start the NetCast listener.
            this.p_NetCast = new NetCast.Queue(12000, 12001);
            this.p_NetCast.OnReceived += new EventHandler<MessageEventArgs>(p_NetCast_OnReceived);

            // Show the system tray icon.
            this.m_TrayIcon = new TrayIcon(this);

            // Set the handler for the offline icon.
            this.m_OffTimer.Tick += new EventHandler(m_OffTimer_Tick);

            // Advertise client service to the server.
            Thread t = new Thread(() =>
                {
                    while (true)
                    {
                        ClientServiceStartingMessage cssm = new ClientServiceStartingMessage(this.p_NetCast.TcpSelf, this.m_Name);
                        cssm.SendUDP(new IPEndPoint(IPAddress.Broadcast, 13000));
                        Thread.Sleep(1000);
                    }
                });
            t.IsBackground = true;
            t.Start();

            // Start the main application loop.
            Application.Run();
        }

        void Application_ApplicationExit(object sender, EventArgs e)
        {
            this.p_NetCast.Stop();
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
                this.m_TrayIcon.Icon = Resources.TrayCountdown;
                MessageBox.Show("Countdown: " + (e.Message as CountdownBroadcastMessage).SecondsRemaining);
            }
            else if (e.Message is ScreenRequestMessage)
            {
                this.m_TrayIcon.Icon = Resources.TrayOn;

                // Send the screen result.
                ScreenResultMessage srm = new ScreenResultMessage(this.p_NetCast.TcpSelf, this.GetScreen());
                srm.SendTCP(e.Message.Source);

                // Use a timer to revert to the off icon.
                this.m_OffTimer.Interval = 1000;
                this.m_OffTimer.Stop();
                this.m_OffTimer.Start();
            }
        }

        void m_OffTimer_Tick(object sender, EventArgs e)
        {
            this.m_TrayIcon.Icon = Resources.TrayOff;
            this.m_OffTimer.Stop();
        }

        public NetCast.Queue NetCast
        {
            get { return this.p_NetCast; }
        }

        public Bitmap GetScreen()
        {
            //try
            //{
                // Use the GDI call to create a DC to the whole display.
                IntPtr dc1 = Gdi.CreateDisplay();
                Graphics g1 = Graphics.FromHdc(dc1);

                /*
                // Get the bitmap to draw on, creating it if necessary.
                if (this.m_Bitmap == null || this.m_Bitmap.Width != Screen.PrimaryScreen.Bounds.Width ||
                    this.m_Bitmap.Height != Screen.PrimaryScreen.Bounds.Height)
                {
                    if (this.m_Bitmap != null)
                        this.m_Bitmap.Dispose();

                    this.m_Bitmap = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, g1);
                }
                */
                Bitmap bitmap = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, g1);
                Graphics g2 = Graphics.FromImage(bitmap);
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

                return bitmap;
            /*}
            catch (InvalidOperationException)
            {
                // Bitmap is probably still in use...
            }*/
        }
    }
}
