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
                        new Rectangle(0, 0, this.ClientSize.Width, this.ClientSize.Height - 64),
                        new Rectangle(0, 0, b.Width, b.Height)
                        );
                r.Location = new Point(r.Location.X, r.Location.Y + 64);
                e.Graphics.DrawImage(b, r, new Rectangle(0, 0, b.Width, b.Height), GraphicsUnit.Pixel);

                // Get a center string style.
                StringFormat center = new StringFormat();
                center.Alignment = StringAlignment.Center;
                center.LineAlignment = StringAlignment.Center;

                // Draw the overlay.
                e.Graphics.FillRectangle(new SolidBrush(Color.White), 0, 0, this.ClientSize.Width, 64);
                e.Graphics.DrawString(
                    "James Rhodes (" + b.Width + "x" + b.Height + ")",
                    new Font(FontFamily.GenericSansSerif, 24, FontStyle.Regular, GraphicsUnit.Pixel),
                    new SolidBrush(Color.Black),
                    new Rectangle(0, 0, this.ClientSize.Width, 64),
                    center
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
    }
}
