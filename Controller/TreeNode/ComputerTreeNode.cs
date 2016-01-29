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
            else if (Computer.Role == Role.Client)
            {
                var child = mainForm.MdiChildren.OfType<ClientEditForm>().FirstOrDefault(x => x.ComputerTreeNode == this);
                if (child != null)
                {
                    child.Focus();
                }
                else
                {
                    var clientEditForm = new ClientEditForm(this.Jam, this);
                    clientEditForm.MdiParent = mainForm;
                    clientEditForm.Show();
                }
            }
        }

        private static DateTime _lastTimeThisWasUpdated;

        public void Update(bool isThreaded = false)
        {
            if (isThreaded)
            {
                TreeView?.Invoke(new Action(() =>
                {
                    if (this.UpdateText() || this.UpdateImage())
                    {
                        if ((DateTime.UtcNow - _lastTimeThisWasUpdated).TotalMilliseconds > 20)
                        {
                            TreeView.Refresh();
                            _lastTimeThisWasUpdated = DateTime.UtcNow;
                        }
                    }
                }));
            }
            else
            {
                if (this.UpdateText() || this.UpdateImage())
                {

                    if (TreeView != null)
                    {
                        if ((DateTime.UtcNow - _lastTimeThisWasUpdated).TotalMilliseconds > 20)
                        {
                            TreeView.Refresh();
                            _lastTimeThisWasUpdated = DateTime.UtcNow;
                        }
                    }
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

        private bool UpdateText()
        {
            var oldText = Text;
            Text = Computer.Hostname;
            if (Computer.WaitingForPing)
            {
                var span = DateTime.UtcNow - Computer.LastContact;
                Text += " (" + (int)Math.Floor(span.TotalMinutes) + ":" + span.Seconds.ToString("D2") + " since last contact)";
            }
            else if (Computer.EmailAddress != null)
            {
                Text += " (" + Computer.EmailAddress + ")";
            }

            return (oldText != Text);
        }

        private bool UpdateImage()
        {
            var oldImage = this.ImageKey;

            if (Computer.WaitingForPing)
            {
                this.ImageKey = @"bullet_red.png";
            }
            else if (!Computer.HasReceivedVersionInformation)
            {
                if (Computer.SentVersionInformation)
                {
                    this.ImageKey = @"transmit.png";
                }
                else
                {
                    this.ImageKey = @"error.png";
                }
            }
            else
            {
                switch (Computer.Role)
                {
                    case Role.Client:
                        {
                            if (Computer.LastTimeControllerSentMessageToBootstrap == null)
                            {
                                this.ImageKey = @"monitor_never_sent.fw.png";
                            }
                            else if (Computer.LastTimeBootstrapSentAMessage < DateTime.UtcNow.AddMinutes(-10))
                            {
                                this.ImageKey = @"monitor_not_responding.fw.png";
                            }
                            else if (Computer.EmailAddress == null)
                            {
                                this.ImageKey = @"monitor_not_authed.png";
                            }
                            else
                            {
                                this.ImageKey = @"monitor.png";
                            }
                            break;
                        }
                    case Role.Projector:
                        this.ImageKey = @"photo.png";
                        break;
                }
            }

            this.SelectedImageKey = this.ImageKey;
            this.StateImageKey = this.ImageKey;

            return (this.ImageKey != oldImage);
        }
    }
}