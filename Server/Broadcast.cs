using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace JamCast
{
    public partial class Broadcast : Form
    {
        private Manager m_Manager = null;
        private readonly DateTime m_End = new DateTime(2011, 01, 30, 15, 0, 0, DateTimeKind.Local);
        private int m_StreamX = 0;

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

        private void Broadcast_Paint(object sender, PaintEventArgs e)
        {
            // Clear our window first.
            e.Graphics.Clear(Color.Black);

            // Get our bitmap data from the Manager.
            Bitmap b = this.m_Manager.Screen;

            if (b != null)
            {
                // .. and draw it.
                Rectangle r = this.ScaleToFit(
                        new Rectangle(0, 0, this.ClientSize.Width, this.ClientSize.Height - 128),
                        new Rectangle(0, 0, b.Width, b.Height)
                        );
                r.Location = new Point(r.Location.X, r.Location.Y + 64);
                e.Graphics.DrawImage(b, r, new Rectangle(0, 0, b.Width, b.Height), GraphicsUnit.Pixel);

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

                // Draw the top overlay.
                e.Graphics.FillRectangle(new SolidBrush(Color.White), 0, 0, this.ClientSize.Width, 64);
                e.Graphics.DrawString(
                    (this.m_Manager.CurrentClient + 1).ToString() + ": " + this.m_Manager.CurrentClientName + " (" + b.Width + "x" + b.Height + ")",
                    new Font(FontFamily.GenericSansSerif, 24, FontStyle.Regular, GraphicsUnit.Pixel),
                    new SolidBrush(Color.Black),
                    new Rectangle(32, 0, this.ClientSize.Width, 64),
                    left
                    );

                // Draw the COUNTDOWN!
                TimeSpan span = new TimeSpan(this.m_End.Ticks - DateTime.Now.Ticks);
                string ms = span.Milliseconds.ToString().PadLeft(4, '0').Substring(1, 3);
                string hrs = (span.Hours + (span.Days * 24)).ToString();
                e.Graphics.DrawString(
                    hrs + " HOURS\n" + span.Minutes + " MINUTES\n" + span.Seconds + "." + ms + " SECS ",
                    new Font(FontFamily.GenericSansSerif, 128 + 32, FontStyle.Regular, GraphicsUnit.Pixel),
                    new SolidBrush(Color.Red),
                    new Rectangle(0, 32, this.ClientSize.Width - 32, this.ClientSize.Height - 64),
                    center
                    );

                // Draw the bottom overlay.
                e.Graphics.FillRectangle(new SolidBrush(Color.Black), 0, this.ClientSize.Height - 64, this.ClientSize.Width, 64);

                // Draw the TWEETS! ~.o
                string st = this.m_Manager.GetTweetStream();
                SizeF size = e.Graphics.MeasureString(st, new Font(FontFamily.GenericSansSerif, 16, FontStyle.Bold, GraphicsUnit.Pixel));

                if (this.m_StreamX < -size.Width + this.ClientSize.Width - 32)
                    this.m_StreamX = 0;
                else
                    this.m_StreamX -= 2;

                //TweetSharp.TwitterSearchStatus tss = this.m_Manager.GetTweet();
                //string a = System.Compat.Web.HttpUtility.HtmlDecode(tss.Author.ScreenName);
                //string t = System.Compat.Web.HttpUtility.HtmlDecode(tss.Text);
                e.Graphics.DrawString(
                    st + st,
                    new Font(FontFamily.GenericSansSerif, 16, FontStyle.Bold, GraphicsUnit.Pixel),
                    new SolidBrush(Color.White),
                    new Rectangle(this.m_StreamX + 32, this.ClientSize.Height - 64, (int)size.Width * 2 + this.ClientSize.Width, 64),
                    left
                    );
            }
            else
            {
                // ... there's no clients.
                StringFormat center = new StringFormat();
                center.Alignment = StringAlignment.Center;
                center.LineAlignment = StringAlignment.Center;

                e.Graphics.DrawString(
                    "No clients connected.",
                    new Font(FontFamily.GenericSansSerif, 24, FontStyle.Regular, GraphicsUnit.Pixel),
                    new SolidBrush(Color.White),
                    new Rectangle(0, 0, this.ClientSize.Width, this.ClientSize.Height),
                    center
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
        }
    }
}
