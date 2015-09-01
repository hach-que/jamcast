#if PLATFORM_WINDOWS

using System;
using System.IO;
using System.Windows.Forms;
using Client.Properties;
using System.Drawing;

namespace Client
{
	public partial class Manager
	{
		private TrayIcon m_TrayIcon = null;
		private Timer m_OffTimer = new Timer();

		private void LoadUsername()
		{
			if (File.Exists("user.txt"))
			{
				using (var reader = new StreamReader("user.txt"))
				{
					this.m_Name = reader.ReadToEnd().Trim();
				}
			}
			else
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

				using (var writer = new StreamWriter("user.txt"))
				{
					writer.Write(this.m_Name);
				}
			}
		}

		private void ListenForApplicationExit(Action onExit)
		{
			// Listen for the application exit event.
			Application.ApplicationExit += (sender, e) => onExit();
		}

		private void ConfigureSystemTrayIcon()
		{
			// Show the system tray icon.
			this.m_TrayIcon = new TrayIcon(this);

			// Set the handler for the offline icon.
			this.m_OffTimer.Tick += new EventHandler(m_OffTimer_Tick);
		}

		private void SetTrayIconToCountdown()
		{
			this.m_TrayIcon.Icon = Resources.TrayCountdown;
		}

		private void SetTrayIconToOff()
		{
			this.m_TrayIcon.Icon = Resources.TrayOff;
		}

		private void SetTrayIconToOn()
		{
			this.m_TrayIcon.Icon = Resources.TrayOn;
		}

		private void ScheduleTrayIconToOff()
		{
			// Use a timer to revert to the off icon.
			this.m_OffTimer.Interval = 1000;
			this.m_OffTimer.Stop();
			this.m_OffTimer.Start();
		}

		void m_OffTimer_Tick(object sender, EventArgs e)
		{
			this.m_TrayIcon.Icon = Resources.TrayOff;
			this.m_OffTimer.Stop();
		}

		public Bitmap GetScreen()
		{
			IntPtr dc1 = Gdi.CreateDisplay();
			Graphics g1 = Graphics.FromHdc(dc1);

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
		}
	}
}

#endif