﻿using System;
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
            this.ContextMenuStrip.Items.Add("Scan for Computers", null, (sender, args) =>
            {
                var mainForm = this.TreeView.FindForm() as MainForm;
                mainForm.SlackController.ScanComputers(this.Jam.Guid);
            });

            this.ImageKey = @"bullet_red.png";
            this.SelectedImageKey = this.ImageKey;
            this.StateImageKey = this.ImageKey;
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
            var status = mainForm.SlackController.GetConnectionStatus(this.Jam.Guid);
            switch (status)
            {
                case SlackConnectionStatus.Connected:
                    this.ImageKey = @"bullet_green.png";
                    break;
                case SlackConnectionStatus.Connecting:
                    this.ImageKey = @"bullet_orange.png";
                    break;
                case SlackConnectionStatus.Disconnected:
                    this.ImageKey = @"bullet_red.png";
                    break;
            }

            this.SelectedImageKey = this.ImageKey;
            this.StateImageKey = this.ImageKey;
        }

        public bool LabelUpdated(string label)
        {
            this.Jam.Name = label;
            this.Jam.Save();

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
            this.Nodes.Add(new ComputerTreeNode(computer));

            if (!this.IsExpanded)
            {
                this.Expand();
            }
        }
    }
}
