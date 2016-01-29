using System;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Controller.TreeNode;
using Newtonsoft.Json;

namespace Controller.Forms
{
    public partial class ClientEditForm : Form
    {
        public ClientEditForm(Jam jam, ComputerTreeNode computerTreeNode)
        {
            Jam = jam;
            ComputerTreeNode = computerTreeNode;

            InitializeComponent();

            Text = ComputerTreeNode.Computer.Hostname;

            _updateTimer_Tick(this, null);
        }

        public Jam Jam { get; private set; }
        public ComputerTreeNode ComputerTreeNode { get; private set; }

        private void _close_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void _updateTimer_Tick(object sender, EventArgs e)
        {
            _emailAddress.Text = ComputerTreeNode.Computer.EmailAddress;
            _fullName.Text = ComputerTreeNode.Computer.FullName;
            _hostname.Text = ComputerTreeNode.Computer.Hostname;
            _ipAddresses.Text = ComputerTreeNode.Computer.IPAddresses.Select(x => x.ToString()).DefaultIfEmpty("None reported").Aggregate((a, b) => a + Environment.NewLine + b);
            _macAddresses.Text = ComputerTreeNode.Computer.MACAddresses.Select(x => x.ToString()).DefaultIfEmpty("None reported").Aggregate((a, b) => a + Environment.NewLine + b);
            _cloudOperations.Text = ComputerTreeNode.Computer.CloudOperationsRequested.ToString();

            _lastTimeBootstrapSentAMessageTextBox.Text = FromDateTime(ComputerTreeNode.Computer.LastTimeBootstrapSentAMessage, "(no last sent message)");
            _lastTimeControllerSentMessageToBootstrapTextBox.Text = FromDateTime(ComputerTreeNode.Computer.LastTimeControllerSentMessageToBootstrap, "(controller never sent message)");
            _lastTimeControllerRecievedMessageFromBootstrapTextBox.Text = FromDateTime(ComputerTreeNode.Computer.LastTimeControllerRecievedMessageFromBootstrap, "(controller never recieved message)");
            _lastTimeBootstrapRecievedAMessageFromControllerAndAckedItTextBox.Text = FromDateTime(ComputerTreeNode.Computer.LastTimeBootstrapRecievedAMessageFromControllerAndAckedIt, "(never acked)");
        }

        private string FromDateTime(DateTime? lastTimeBootstrapSentAMessage, string v)
        {
            if (lastTimeBootstrapSentAMessage == null)
            {
                return v;
            }
            else
            {
                return lastTimeBootstrapSentAMessage.Value.ToLocalTime().ToString();
            }
        }

        private void ClientEditForm_Load(object sender, EventArgs e)
        {

        }
    }
}