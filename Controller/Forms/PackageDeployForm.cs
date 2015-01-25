using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Controller.Forms
{
    public partial class PackageDeployForm : Form
    {
        public PackageDeployForm()
        {
            InitializeComponent();
        }

        public int Progress
        {
            set { this.c_ProgressBar.Value = value; }
        }
    }
}
