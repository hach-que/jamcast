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
            // Get our bitmap data from the Manager.
            Bitmap b = this.m_Manager.Screen;

            // .. and draw it.
            e.Graphics.DrawImage(b, new Rectangle(0, 0, this.Width, this.Height), new Rectangle(0, 0, b.Width, b.Height), GraphicsUnit.Pixel);
        }
    }
}
