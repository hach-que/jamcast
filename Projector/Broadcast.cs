using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Projector;

namespace JamCast
{
    public partial class Broadcast : Form
    {
        private Manager m_Manager = null;
        private int m_StreamX = 0;
        private Random m_Random = new Random();

        public Broadcast(Manager manager)
        {
            InitializeComponent();

            // Set our manager.
            this.m_Manager = manager;

            // Set it up so that all drawing is done through our code.
            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.UserPaint, true);
        }

		public void AddControl(Control c)
		{
			this.Controls.Add (c);
		}

        private void Broadcast_Paint(object sender, PaintEventArgs e)
        {
            // Hide mouse cursor.
            Cursor.Hide();

            // Clear our window first.
            e.Graphics.Clear(Color.Black);

            // Get our bitmap data from the Manager.
            BitmapTracker.Purge();

            // Get a center string style.
            StringFormat left = new StringFormat();
            left.Alignment = StringAlignment.Near;
            left.LineAlignment = StringAlignment.Center;
            StringFormat right = new StringFormat();
            right.Alignment = StringAlignment.Far;
            right.LineAlignment = StringAlignment.Center;
            StringFormat center = new StringFormat();
            center.Alignment = StringAlignment.Center;
            center.LineAlignment = StringAlignment.Center;
            StringFormat tleft = new StringFormat();
            tleft.Alignment = StringAlignment.Near;
            tleft.LineAlignment = StringAlignment.Near;

            var currentClient = this.m_Manager.CurrentClientObject;
            if (currentClient != null)
            {
                if (currentClient.FfplayProcess != null && !currentClient.FfplayProcess.HasExited)
                {
                    FfplayStreamController.AlignToFormBounds(
                        currentClient.FfplayProcess,
                        this,
                        new Rectangle(0, 64, this.ClientSize.Width - (AppSettings.SlackEnabled ? 256 : 0), this.ClientSize.Height - 128));
                }
            }
            else
            {
                // ... there's no clients.
                e.Graphics.DrawString(
                    "Waiting on clients...",
                    new Font(FontFamily.GenericSansSerif, 24, FontStyle.Regular, GraphicsUnit.Pixel),
                    new SolidBrush(Color.White),
                    new Rectangle(0, 0, this.ClientSize.Width, this.ClientSize.Height),
                    center
                    );
            }

            /*
            if (b != null)
            {
                try
                {
                    // .. and draw it.
                    Rectangle r = this.ScaleToFit(
                            new Rectangle(0, 0, this.ClientSize.Width - (AppSettings.EnableChat ? 256 : 0), this.ClientSize.Height - 128),
                            new Rectangle(0, 0, b.Width, b.Height)
                            );
                    r.Location = new Point(r.Location.X, r.Location.Y + 64);
                    e.Graphics.DrawImage(b, r, new Rectangle(0, 0, b.Width, b.Height), GraphicsUnit.Pixel);
                    e.Graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                    // Draw the top overlay.
                    e.Graphics.FillRectangle(new SolidBrush(Color.White), 0, 0, this.ClientSize.Width, 64);
                    e.Graphics.DrawString(
                        (this.m_Manager.CurrentClient + 1).ToString() + ": " + this.m_Manager.CurrentClientName + " (" + b.Width + "x" + b.Height + ")",
                        new Font(FontFamily.GenericSansSerif, 24, FontStyle.Regular, GraphicsUnit.Pixel),
                        new SolidBrush(Color.Black),
                        new Rectangle(32, 0, this.ClientSize.Width, 64),
                        left
                        );
                }
                catch (Exception)
                {
                    // ... there's no clients.
                    e.Graphics.DrawString(
                        "Failed to render known clients...",
                        new Font(FontFamily.GenericSansSerif, 24, FontStyle.Regular, GraphicsUnit.Pixel),
                        new SolidBrush(Color.White),
                        new Rectangle(0, 0, this.ClientSize.Width, this.ClientSize.Height),
                        center
                        );
                }
            }
            else
            {
            }*/

            if (AppSettings.SlackEnabled)
            {
                // Draw the chat.
                List<string> chat = this.m_Manager.GetChatStream();
                string d = null;
                if (chat.Count == 0)
                    d = "Showing messages from the " + string.Join(", ", AppSettings.SlackChannels.Select(x => "#" + x)) + " slack channels.";
                else
                    d = chat.Reverse<string>().Aggregate((a2, b2) => a2 + "\r\n" + b2);
                e.Graphics.FillRectangle(new SolidBrush(Color.White), this.ClientSize.Width - 256 + 16, 64, 256, this.ClientSize.Height - 128);
                e.Graphics.DrawString(
                    d,
                    new Font(FontFamily.GenericSansSerif, 14, FontStyle.Regular, GraphicsUnit.Pixel),
                    new SolidBrush(Color.Black),
                    new Rectangle(this.ClientSize.Width - 256 + 16, 64, 256 - 32, this.ClientSize.Height - 128 - 32),
                    tleft
                    );
            }

            // Draw memory usage (top-middle)
            string mem = (Process.GetCurrentProcess().WorkingSet64 / 1024 / 1024).ToString() + "MB";
            e.Graphics.DrawString(
                mem,
                new Font(FontFamily.GenericSansSerif, 10, FontStyle.Regular, GraphicsUnit.Pixel),
                new SolidBrush(Color.Gray),
                new Rectangle(this.ClientSize.Width / 2 - 64, 0, 128, 64),
                center
                );

            // Draw the COUNTDOWN! (top-right)
            TimeSpan span = new TimeSpan(AppSettings.EndTime.Ticks - DateTime.Now.Ticks);
            string ms = span.Milliseconds.ToString().PadLeft(4, '0').Substring(1, 3);
            string hrs = (span.Hours + (span.Days * 24)).ToString();
            string sstr = hrs + " hours " + span.Minutes + " minutes " + span.Seconds + "." + ms + " seconds ";
            if (span.Ticks <= 0)
                sstr = "GAME JAM OVER!";
            e.Graphics.DrawString(
                sstr,
                new Font(FontFamily.GenericSansSerif, 24, FontStyle.Regular, GraphicsUnit.Pixel),
                new SolidBrush(Color.Red),
                new Rectangle(0, 0, this.ClientSize.Width - 32, 64),
                right
                );

            if (span.Hours < 2)
            {
                string str = "";
                if (span.Ticks < 0)
                    str = "GAME JAM OVER!";
                else
                    str = hrs + " HOURS\n" + span.Minutes + " MINUTES\n" + span.Seconds + "." + ms + " SECS ";

                if (span.Hours > 1 || Math.Floor((double)span.Milliseconds / 500) % 2 == 0 || span.Seconds < 0)
                {
                    // Draw the COUNTDOWN! (center)
                    e.Graphics.DrawString(
                        str,
                        new Font(FontFamily.GenericSansSerif, 128, FontStyle.Regular, GraphicsUnit.Pixel),
                        new SolidBrush(Color.FromArgb(200 + this.m_Random.Next(32), 0, 0)),
                        new PointF(this.ClientSize.Width / 2, this.ClientSize.Height / 2),
                        center
                        );
                }
                else
                {
                    // Draw the COUNTDOWN! (center)
                    e.Graphics.DrawString(
                        str,
                        new Font(FontFamily.GenericSansSerif, 128, FontStyle.Regular, GraphicsUnit.Pixel),
                        new SolidBrush(Color.White),
                        new PointF(this.ClientSize.Width / 2, this.ClientSize.Height / 2),
                        center
                        );
                }

                // Draw border.
                GraphicsPath gp = new GraphicsPath();
                gp.AddString(
                    str,
                    FontFamily.GenericSansSerif,
                    (int)FontStyle.Regular,
                    128,
                    new PointF(this.ClientSize.Width / 2, this.ClientSize.Height / 2),
                    center
                    );
                e.Graphics.DrawPath(new Pen(Brushes.Black, 2), gp);
            }

            // Draw the bottom overlay.
            e.Graphics.FillRectangle(new SolidBrush(Color.Black), 0, this.ClientSize.Height - 64, this.ClientSize.Width, 64);

            if (AppSettings.TwitterEnabled)
            {
                // Draw the TWEETS! ~.o
                string st = this.m_Manager.GetTweetStream();
                SizeF size = e.Graphics.MeasureString(st,
                    new Font(FontFamily.GenericSansSerif, 16, FontStyle.Bold, GraphicsUnit.Pixel));

                if (this.m_StreamX < -size.Width + this.ClientSize.Width - 32)
                    this.m_StreamX = 0;
                else
                    this.m_StreamX -= 2;

                e.Graphics.DrawString(
                    (st + st).Replace("\n", "").Replace("\r", ""),
                    new Font(FontFamily.GenericSansSerif, 16, FontStyle.Bold, GraphicsUnit.Pixel),
                    new SolidBrush(Color.White),
                    //new Point(this.m_StreamX + 32, this.ClientSize.Height - 64),
                    new Rectangle(this.m_StreamX + 32, this.ClientSize.Height - 64,
                        (int) size.Width*9 + this.ClientSize.Width, 64),
                    left
                    );
            }
        }

        /// <summary>
        /// Scales a rectangle to fit inside another rectangle.
        /// </summary>
        /// <param name="outside">The rectangle to fit 'inside' into.</param>
        /// <param name="inside">The other rectangle to adapt to fit inside 'outside'.</param>
        /// <returns>The scaled and centered rectangle.</returns>
        private Rectangle ScaleToFit(Rectangle outside, Rectangle inside)
        {
            double outerRatio = (double)outside.Height / (double)outside.Width;
            double innerRatio = (double)inside.Height / (double)inside.Width;
            Rectangle result = new Rectangle(0, 0, outside.Width, outside.Height);

            if (innerRatio > outerRatio)
            {
                // Tallscreen; make the height reach the boundaries.
                double d = (double)inside.Height / (double)outside.Height;
                result.Width = (int)(inside.Width / d);
                result.Height = outside.Height;
            }
            else if (innerRatio < outerRatio)
            {
                // Widescreen; make the width reach the boundaries.
                double d = (double)inside.Width / (double)outside.Width;
                result.Width = outside.Width;
                result.Height = (int)(inside.Height / d);
            }
            else
            {
                // Same aspect ratio.
                result.Width = outside.Width;
                result.Height = outside.Height;
            }

            // Adjust the position.
            result.X = (outside.Width - result.Width) / 2;
            result.Y = (outside.Height - result.Height) / 2;

            return result;
        }

        private void Broadcast_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                // Shutdown.
                Application.Exit();
            }
            else if (e.KeyCode == Keys.Space)
            {
                // Next client.
                this.m_Manager.NextClient();
            }
        }
    }
}
