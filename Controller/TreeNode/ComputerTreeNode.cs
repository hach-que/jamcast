using System;
using System.Windows.Forms;

namespace Controller.TreeNode
{
    public class ComputerTreeNode : System.Windows.Forms.TreeNode, IDoubleClick
    {
        public ComputerTreeNode(Computer computer)
        {
            if (computer == null)
            {
                throw new ArgumentNullException("computer");
            }

            Computer = computer;
            Update();

            ContextMenuStrip = new ContextMenuStrip();
            this.ContextMenuStrip.Items.Add("Designate as Client", null, (sender, args) =>
            {
                Computer.Role = Role.Client;
                this.UpdateImage();
            });
            this.ContextMenuStrip.Items.Add("Designate as Projector", null, (sender, args) =>
            {
                Computer.Role = Role.Projector;
                this.UpdateImage();
            });

            this.UpdateImage();
        }

        public Computer Computer { get; private set; }

        public void DoubleClicked(MainForm mainForm)
        {
        }

        public void Update()
        {
            Text = Computer.Hostname;
            this.UpdateImage();
        }

        private void UpdateImage()
        {
            switch (Computer.Role)
            {
                case Role.Client:
                    this.ImageKey = @"monitor.png";
                    break;
                case Role.Projector:
                    this.ImageKey = @"photo.png";
                    break;
            }

            this.SelectedImageKey = this.ImageKey;
            this.StateImageKey = this.ImageKey;
        }
    }
}