using System;
using System.Windows.Forms;

namespace Client
{
    public partial class WhoAreYou : Form
    {
        public WhoAreYou()
        {
            InitializeComponent();
        }

        private void c_NameTextBox_TextChanged(object sender, EventArgs e)
        {
            string trim = this.c_NameTextBox.Text.Trim();
            if (trim.Length > 0)
                this.c_GoButton.Enabled = true;
            else
                this.c_GoButton.Enabled = false;
        }

        private void c_CancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void c_GoButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        public string Name
        {
            get { return this.c_NameTextBox.Text.Trim(); }
        }
    }
}
