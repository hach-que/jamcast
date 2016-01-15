using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Controller.Forms;

namespace Controller.TreeNode
{
    public class ComputerTreeNode : System.Windows.Forms.TreeNode, IDoubleClick
    {
        public ComputerTreeNode(Jam jam, Computer computer)
        {
            if (computer == null)
            {
                throw new ArgumentNullException("computer");
            }

            Computer = computer;
            Jam = jam;
            Update();

            ContextMenuStrip = new ContextMenuStrip();
            this.ContextMenuStrip.Items.Add("Designate as Client", null, (sender, args) =>
            {
                Computer.Role = Role.Client;
                Computer.WaitingForPing = true;
                this.UpdateImage();

                var mainForm = this.TreeView.FindForm() as MainForm;
                mainForm.PubSubController.DesignateComputer(Jam.Guid, Computer.Guid, Computer.Role);
            });
            this.ContextMenuStrip.Items.Add("Designate as Projector", null, (sender, args) =>
            {
                Computer.Role = Role.Projector;
                Computer.WaitingForPing = true;
                this.UpdateImage();

                var mainForm = this.TreeView.FindForm() as MainForm;
                mainForm.PubSubController.DesignateComputer(Jam.Guid, Computer.Guid, Computer.Role);
            });
            this.ContextMenuStrip.Items.Add("Show IP Addresses", null, (sender, args) =>
            {
                var addresses = string.Join("\r\n", Computer.IPAddresses.Select(x => x.ToString()));
                MessageBox.Show(addresses);
            });

            this.UpdateImage();
        }

        public Jam Jam { get; private set; }

        public Computer Computer { get; private set; }

        public void DoubleClicked(MainForm mainForm)
        {
            if (Computer.Role == Role.Projector)
            {
                var child = mainForm.MdiChildren.OfType<ProjectorEditForm>().FirstOrDefault(x => x.ComputerTreeNode == this);
                if (child != null)
                {
                    child.Focus();
                }
                else
                {
                    var projectorEditForm = new ProjectorEditForm(this.Jam, this);
                    projectorEditForm.MdiParent = mainForm;
                    projectorEditForm.Show();
                }
            }
        }

        public void Update(bool isThreaded = false)
        {
            if (isThreaded)
            {
                TreeView?.Invoke(new Action(() =>
                {
                    this.UpdateText();
                    this.UpdateImage();

                    TreeView.Refresh();
                }));
            }
            else
            {
                this.UpdateText();
                this.UpdateImage();

                if (TreeView != null)
                {
                    TreeView.Refresh();
                }
            }

            if (Computer.WaitingForPing)
            {
                if (!Computer.HasWaitingTask)
                {
                    Computer.HasWaitingTask = true;
                    Task.Run(async () =>
                    {
                        while (Computer.WaitingForPing)
                        {
                            await Task.Delay(1000);
                            this.Update(true);
                        }
                    });
                }
            }
            else if (Computer.HasWaitingTask)
            {
                Computer.HasWaitingTask = false;
            }
        }

        private void UpdateText()
        {
            Text = Computer.Hostname;
            if (Computer.WaitingForPing)
            {
                var span = DateTime.UtcNow - Computer.LastContact;
                Text += " (" + (int)Math.Floor(span.TotalMinutes) + ":" + span.Seconds.ToString("D2") + " since last contact)";
            }
        }

        private void UpdateImage()
        {
            if (Computer.WaitingForPing)
            {
                this.ImageKey = @"bullet_red.png";
            }
            else if (!Computer.HasReceivedVersionInformation)
            {
                this.ImageKey = @"error.png";
            }
            else
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
            }

            this.SelectedImageKey = this.ImageKey;
            this.StateImageKey = this.ImageKey;
        }
    }
}