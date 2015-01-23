using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Controller.TreeNode;

namespace Controller.Forms
{
    public partial class JamEditForm : Form
    {
        public JamTreeNode JamTreeNode { get; private set; }

        public JamEditForm(JamTreeNode jamTreeNode)
        {
            JamTreeNode = jamTreeNode;
            
            InitializeComponent();

            this.Text = JamTreeNode.Jam.Name;
            this.c_ControllerSlackAPIToken.Text = JamTreeNode.Jam.ControllerSlackToken;
            this.c_ProjectorSlackChannels.Text = JamTreeNode.Jam.ProjectorSlackChannels;
            this.c_ClientSlackAPIToken.Text = JamTreeNode.Jam.ClientSlackToken;

            this.RefreshConnectionStatus();
        }

        public void NameChanged()
        {
            this.Text = JamTreeNode.Jam.Name;
        }

        private void c_SlackAPIToken_TextChanged(object sender, System.EventArgs e)
        {
            this.JamTreeNode.Jam.ControllerSlackToken = this.c_ControllerSlackAPIToken.Text;
            this.JamTreeNode.Jam.Save();
        }

        private void c_ProjectorSlackChannels_TextChanged(object sender, System.EventArgs e)
        {
            this.JamTreeNode.Jam.ProjectorSlackChannels = this.c_ProjectorSlackChannels.Text;
            this.JamTreeNode.Jam.Save();
        }

        private void c_ClientSlackAPIToken_TextChanged(object sender, System.EventArgs e)
        {
            this.JamTreeNode.Jam.ClientSlackToken = this.c_ClientSlackAPIToken.Text;
            this.JamTreeNode.Jam.Save();
        }

        private void JamEditForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.JamTreeNode.Jam.ControllerSlackToken = this.c_ControllerSlackAPIToken.Text;
            this.JamTreeNode.Jam.ProjectorSlackChannels = this.c_ProjectorSlackChannels.Text;
            this.JamTreeNode.Jam.ClientSlackToken = this.c_ClientSlackAPIToken.Text;
            this.JamTreeNode.Jam.Save();
        }

        private void c_CreateBootstrap_Click(object sender, System.EventArgs e)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.FileName = "Bootstrap.exe";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    using (var writer = new FileStream(sfd.FileName, FileMode.Create))
                    {
                        var bytes = BootstrapCreator.CreateCustomBootstrap(this.c_ClientSlackAPIToken.Text);
                        writer.Write(bytes, 0, bytes.Length);
                        writer.Flush();
                    }

                    var path = new FileInfo(sfd.FileName).Directory.FullName;

                    foreach (var file in new FileInfo(typeof (Program).Assembly.Location).Directory.GetFiles("*.dll"))
                    {
                        file.CopyTo(Path.Combine(path, file.Name), true);
                    }

                    foreach (var file in new FileInfo(typeof(Program).Assembly.Location).Directory.GetFiles("*.pdb"))
                    {
                        file.CopyTo(Path.Combine(path, file.Name), true);
                    }

                    foreach (var file in new FileInfo(typeof(Program).Assembly.Location).Directory.GetFiles("*.dll.config"))
                    {
                        file.CopyTo(Path.Combine(path, file.Name), true);
                    }

                    using (var writer = new StreamWriter(sfd.FileName + ".config", false))
                    {
                        writer.Write(@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<configuration>
  <runtime>
    <assemblyBinding xmlns=""urn:schemas-microsoft-com:asm.v1"">
      <dependentAssembly>
        <assemblyIdentity name=""Newtonsoft.Json"" publicKeyToken=""30AD4FE6B2A6AEED"" culture=""neutral""/>
        <bindingRedirect oldVersion=""0.0.0.0-6.0.0.0"" newVersion=""6.0.0.0""/>
      </dependentAssembly>
    </assemblyBinding>
  </runtime>
</configuration>");
                    }
                }
            }
        }

        public void RefreshConnectionStatus()
        {
            var mainForm = JamTreeNode.TreeView.FindForm() as MainForm;
            var status = mainForm.SlackController.GetConnectionStatus(JamTreeNode.Jam.Guid);
            switch (status)
            {
                case SlackConnectionStatus.Connected:
                    this.m_SlackConnectionStatus.Text = "Connected to Slack!";
                    this.m_SlackConnectionStatus.BackColor = Color.DarkSeaGreen;
                    break;
                case SlackConnectionStatus.Connecting:
                    this.m_SlackConnectionStatus.Text = "Connecting to Slack...";
                    this.m_SlackConnectionStatus.BackColor = Color.LightYellow;
                    break;
                case SlackConnectionStatus.Disconnected:
                    this.m_SlackConnectionStatus.Text = "Not connected to Slack!";
                    this.m_SlackConnectionStatus.BackColor = Color.LightCoral;
                    break;
            }
        }
    }
}