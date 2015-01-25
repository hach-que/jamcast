using System;
using System.Windows.Forms;
using Controller.TreeNode;
using Newtonsoft.Json;

namespace Controller.Forms
{
    public partial class ProjectorEditForm : Form
    {
        public ProjectorEditForm(Jam jam, ComputerTreeNode computerTreeNode)
        {
            Jam = jam;
            ComputerTreeNode = computerTreeNode;

            InitializeComponent();

            Text = ComputerTreeNode.Computer.Hostname;

            if (Jam.CachedProjectorSettings.ContainsKey(ComputerTreeNode.Computer.Guid.ToString()))
            {
                var settings =
                    JsonConvert.DeserializeObject<dynamic>(
                        Jam.CachedProjectorSettings[ComputerTreeNode.Computer.Guid.ToString()]);

                c_TwitterConsumerKey.Text = settings.TwitterConsumerKey;
                c_TwitterConsumerSecret.Text = settings.TwitterConsumerSecret;
                c_TwitterOAuthToken.Text = settings.TwitterOAuthToken;
                c_TwitterOAuthSecret.Text = settings.TwitterOAuthSecret;
                c_TwitterSearchQuery.Text = settings.TwitterSearchQuery;
                c_ProjectorName.Text = settings.ProjectorName;
                c_IsPrimaryProjector.Checked = settings.IsPrimary;
            }
        }

        public Jam Jam { get; private set; }
        public ComputerTreeNode ComputerTreeNode { get; private set; }

        private void JamEditForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SettingsUpdated(sender, e);
        }

        private void SettingsUpdated(object sender, EventArgs e)
        {
            var settingsObject = new
            {
                TwitterConsumerKey = c_TwitterConsumerKey.Text,
                TwitterConsumerSecret = c_TwitterConsumerSecret.Text,
                TwitterOAuthToken = c_TwitterOAuthToken.Text,
                TwitterOAuthSecret = c_TwitterOAuthSecret.Text,
                TwitterSearchQuery = c_TwitterSearchQuery.Text,
                ProjectorName = c_ProjectorName.Text,
                SlackToken = Jam.ClientSlackToken,
                SlackChannels = Jam.ProjectorSlackChannels,
                IsPrimary = c_IsPrimaryProjector.Checked
            };

            var settingsJson = JsonConvert.SerializeObject(settingsObject);

            Jam.CachedProjectorSettings[ComputerTreeNode.Computer.Guid.ToString()] = settingsJson;

            var mainForm = this.MdiParent as MainForm;
            mainForm.SlackController.UpdateComputerSettings(Jam.Guid, ComputerTreeNode.Computer.Guid, "projector-settings", settingsJson);
        }
    }
}