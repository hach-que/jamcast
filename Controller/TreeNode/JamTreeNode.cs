using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Controller.Forms;

namespace Controller.TreeNode
{
    public class JamTreeNode : System.Windows.Forms.TreeNode, IAfterEdit, IDoubleClick
    {
        public JamTreeNode(Jam jam)
        {
            if (jam == null)
            {
                throw new ArgumentNullException("jam");
            }

            this.Jam = jam;
            this.Update();

            this.ContextMenuStrip = new ContextMenuStrip();
            this.ContextMenuStrip.Items.Add("Rename", null, (sender, args) =>
            {
                this.BeginEdit();
            });
            this.ContextMenuStrip.Items.Add("Sync Computers", null, (sender, args) =>
            {
                this.MarkComputersAsWaitingForPing();

                var mainForm = this.TreeView.FindForm() as MainForm;
                mainForm.PubSubController.ScanComputers(this.Jam.Guid);
            });
            this.ContextMenuStrip.Items.Add("Push Client IPs to Projectors", null, (sender, args) =>
            {
                var iplist = new List<object>();

                foreach (var node in this.Nodes.OfType<ComputerTreeNode>())
                {
                    if (node.Computer.Role == Role.Client)
                    {
                        foreach (var ip in node.Computer.IPAddresses)
                        {
                            iplist.Add(new { IPAddress = ip.ToString(), Name = node.Computer.Hostname });
                        }
                    }
                }

                var data = new
                {
                    Type = "ipaddress-list",
                    IPAddresses = iplist,
                };

                var mainForm = this.TreeView.FindForm() as MainForm;
                mainForm.PubSubController.SendCustomMessage(this.Jam.Guid, data);
            });

            this.ImageKey = @"bullet_red.png";
            this.SelectedImageKey = this.ImageKey;
            this.StateImageKey = this.ImageKey;
        }

        public void MarkComputersAsWaitingForPing()
        {
            foreach (var node in this.Nodes.OfType<ComputerTreeNode>())
            {
                node.Computer.WaitingForPing = true;
                node.Update();
            }
        }

        public Jam Jam { get; private set; }

        public void Update()
        {
            this.Text = this.Jam.Name;
            this.UpdateImage();
        }

        public void UpdateImage()
        {
            if (this.TreeView == null)
            {
                return;
            }

            var mainForm = this.TreeView.FindForm() as MainForm;
            var status = mainForm.PubSubController.GetConnectionStatus(this.Jam.Guid);
            switch (status)
            {
                case PubSubConnectionStatus.Connected:
                    this.ImageKey = @"bullet_green.png";
                    break;
                case PubSubConnectionStatus.Connecting:
                    this.ImageKey = @"bullet_orange.png";
                    break;
                case PubSubConnectionStatus.Disconnected:
                    this.ImageKey = @"bullet_red.png";
                    break;
            }

            this.SelectedImageKey = this.ImageKey;
            this.StateImageKey = this.ImageKey;
        }

        public bool LabelUpdated(string label)
        {
            if (string.IsNullOrWhiteSpace(label))
            {
                this.Update();
                return false;
            }

            this.Jam.Name = label;
            this.Jam.Save();

            this.Update();

            var form = this.TreeView.FindForm();
            if (form != null)
            {
                var child = form.MdiChildren.OfType<JamEditForm>().FirstOrDefault(x => x.JamTreeNode == this);
                if (child != null)
                {
                    child.NameChanged();
                }
            }

            return true;
        }

        public void DoubleClicked(MainForm mainForm)
        {
            var child = mainForm.MdiChildren.OfType<JamEditForm>().FirstOrDefault(x => x.JamTreeNode == this);
            if (child != null)
            {
                child.Focus();
            }
            else
            {
                var jamEditForm = new JamEditForm(this);
                jamEditForm.MdiParent = mainForm;
                jamEditForm.Show();
            }
        }

        public void NewComputerRegistered(Computer computer)
        {
            this.Nodes.Add(new ComputerTreeNode(Jam, computer));

            if (!this.IsExpanded)
            {
                this.Expand();
            }
        }
    }
}
