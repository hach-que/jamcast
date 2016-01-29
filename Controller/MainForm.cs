using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Controller.Forms;
using Controller.TreeNode;

namespace Controller
{
    public partial class MainForm : Form
    {
        public PubSubController PubSubController { get; private set; }

        public MainForm()
        {
            this.PubSubController = new PubSubController(this);

            InitializeComponent();

            foreach (var files in new DirectoryInfo(Environment.CurrentDirectory).GetFiles("*.jam"))
            {
                var jam = Jam.Load(files.FullName);
                jam._pubSubController = PubSubController;
                var node = new JamTreeNode(jam);
                jam.SetTreeNode(node);
                c_JamHierarchy.Nodes.Add(node);
                this.PubSubController.RegisterJam(jam);
            }
        }

        private void newJamToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var jam = new Jam(Guid.NewGuid(), "My New Jam");
            jam._pubSubController = PubSubController;
            jam.Save();
            var node = new JamTreeNode(jam);
            jam.SetTreeNode(node);
            c_JamHierarchy.Nodes.Add(node);
            this.PubSubController.RegisterJam(jam);
        }

        private void c_JamHierarchy_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Node is IAfterEdit)
            {
                e.CancelEdit = !(e.Node as IAfterEdit).LabelUpdated(e.Label);
            }
        }

        private void c_JamHierarchy_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node is IDoubleClick)
            {
                (e.Node as IDoubleClick).DoubleClicked(this);
            }
        }

        public void RefreshConnectionStatus(Jam jam)
        {
            var jamTreeNode = this.c_JamHierarchy.Nodes.OfType<JamTreeNode>().FirstOrDefault(x => x.Jam == jam);
            if (jamTreeNode != null)
            {
                jamTreeNode.UpdateImage();
            }

            var child = this.MdiChildren.OfType<JamEditForm>().FirstOrDefault(x => x.JamTreeNode.Jam == jam);
            if (child != null)
            {
                child.RefreshConnectionStatus();
            }
        }
    }

    internal interface IDoubleClick
    {
        void DoubleClicked(MainForm mainForm);
    }

    internal interface IAfterEdit
    {
        bool LabelUpdated(string label);
    }
}