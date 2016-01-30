using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Projector;
using Projector.Controllers;

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

        private void Broadcast_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                // Hide mouse cursor.
                Cursor.Hide();

                // Clear our window first.
                e.Graphics.Clear(Color.Black);

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

                if (m_Manager._ffmpegProcessController != null &&
                    m_Manager._ffmpegProcessController.FfplayProcess != null
                        && !m_Manager._ffmpegProcessController.FfplayProcess.HasExited)
                {
                    FfmpegStreamAPI.AlignToFormBounds(
                        m_Manager._ffmpegProcessController.FfplayProcess,
                        this,
                        new Rectangle(
                            0,
                            64,
                            this.ClientSize.Width - (AppSettings.SlackEnabled ? 256 : 0),
                            this.ClientSize.Height - 128));
                }
                else if (m_Manager._ffmpegProcessController.WaitingOn != null)
                {
                    e.Graphics.DrawString(
                        "Waiting on " + m_Manager._ffmpegProcessController.WaitingOn + "'s computer to stream...",
                        new Font(FontFamily.GenericSansSerif, 24, FontStyle.Regular, GraphicsUnit.Pixel),
                        new SolidBrush(Color.White),
                        new Rectangle(0, 0, this.ClientSize.Width, this.ClientSize.Height),
                        center);
                }
                else
                {
                    e.Graphics.DrawString(
                        "Finding a computer to stream from...",
                        new Font(FontFamily.GenericSansSerif, 24, FontStyle.Regular, GraphicsUnit.Pixel),
                        new SolidBrush(Color.White),
                        new Rectangle(0, 0, this.ClientSize.Width, this.ClientSize.Height),
                        center);
                }
                
                // Draw connection status.
                if (m_Manager._pubSubController != null)
                {
                    string connectionStatus = m_Manager._pubSubController.Status.ToString();
                    e.Graphics.DrawString(
                        connectionStatus,
                        new Font(FontFamily.GenericSansSerif, 10, FontStyle.Regular, GraphicsUnit.Pixel),
                        new SolidBrush(Color.Gray),
                        new Rectangle(this.ClientSize.Width / 2 - 64, 0, 128, 64),
                        center);
                }

                // Draw the COUNTDOWN! (top-right)
                TimeSpan span = new TimeSpan(AppSettings.EndTime.Ticks - DateTime.Now.Ticks);
                string ms = span.Milliseconds.ToString().PadLeft(4, '0').Substring(1, 3);
                string hrs = (span.Hours + (span.Days * 24)).ToString();
                string sstr = hrs + " hours " + span.Minutes + " minutes " + span.Seconds + "." + ms + " seconds ";
                if (span.Ticks <= 0) sstr = "GAME JAM OVER!";
                e.Graphics.DrawString(
                    sstr,
                    new Font(FontFamily.GenericSansSerif, 24, FontStyle.Regular, GraphicsUnit.Pixel),
                    new SolidBrush(Color.Red),
                    new Rectangle(0, 0, this.ClientSize.Width - 32, 64),
                    right);

                if (span.TotalHours < 2)
                {
                    string str = "";
                    if (span.Ticks < 0) str = "GAME JAM OVER!";
                    else str = hrs + " HOURS\n" + span.Minutes + " MINUTES\n" + span.Seconds + "." + ms + " SECS ";

                    if (span.TotalHours > 1 || Math.Floor((double)span.Milliseconds / 500) % 2 == 0 || span.Seconds < 0)
                    {
                        // Draw the COUNTDOWN! (center)
                        e.Graphics.DrawString(
                            str,
                            new Font(FontFamily.GenericSansSerif, 128, FontStyle.Regular, GraphicsUnit.Pixel),
                            new SolidBrush(Color.FromArgb(200 + this.m_Random.Next(32), 0, 0)),
                            new PointF(this.ClientSize.Width / 2, this.ClientSize.Height / 2),
                            center);
                    }
                    else
                    {
                        // Draw the COUNTDOWN! (center)
                        e.Graphics.DrawString(
                            str,
                            new Font(FontFamily.GenericSansSerif, 128, FontStyle.Regular, GraphicsUnit.Pixel),
                            new SolidBrush(Color.White),
                            new PointF(this.ClientSize.Width / 2, this.ClientSize.Height / 2),
                            center);
                    }

                    // Draw border.
                    GraphicsPath gp = new GraphicsPath();
                    gp.AddString(
                        str,
                        FontFamily.GenericSansSerif,
                        (int)FontStyle.Regular,
                        128,
                        new PointF(this.ClientSize.Width / 2, this.ClientSize.Height / 2),
                        center);
                    e.Graphics.DrawPath(new Pen(Brushes.Black, 2), gp);
                }

                // Draw the bottom overlay.
                e.Graphics.FillRectangle(
                    new SolidBrush(Color.Black),
                    0,
                    this.ClientSize.Height - 64,
                    this.ClientSize.Width,
                    64);

                if (AppSettings.TwitterEnabled)
                {
                    // Draw the TWEETS! ~.o
                    /* string st = this.m_Manager.GetTweetStream();
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
                    );*/
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
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
