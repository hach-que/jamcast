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
