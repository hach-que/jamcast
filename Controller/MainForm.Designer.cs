namespace Controller
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.c_JamHierarchy = new System.Windows.Forms.TreeView();
            this.c_TreeContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.newJamToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.c_TreeContextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // c_JamHierarchy
            // 
            this.c_JamHierarchy.ContextMenuStrip = this.c_TreeContextMenu;
            this.c_JamHierarchy.Dock = System.Windows.Forms.DockStyle.Left;
            this.c_JamHierarchy.ImageKey = "bullet_red.png";
            this.c_JamHierarchy.ImageList = this.imageList1;
            this.c_JamHierarchy.LabelEdit = true;
            this.c_JamHierarchy.Location = new System.Drawing.Point(0, 0);
            this.c_JamHierarchy.Name = "c_JamHierarchy";
            this.c_JamHierarchy.SelectedImageIndex = 0;
            this.c_JamHierarchy.Size = new System.Drawing.Size(510, 541);
            this.c_JamHierarchy.TabIndex = 1;
            this.c_JamHierarchy.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.c_JamHierarchy_AfterLabelEdit);
            this.c_JamHierarchy.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.c_JamHierarchy_NodeMouseDoubleClick);
            // 
            // c_TreeContextMenu
            // 
            this.c_TreeContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newJamToolStripMenuItem});
            this.c_TreeContextMenu.Name = "c_TreeContextMenu";
            this.c_TreeContextMenu.Size = new System.Drawing.Size(123, 26);
            // 
            // newJamToolStripMenuItem
            // 
            this.newJamToolStripMenuItem.Name = "newJamToolStripMenuItem";
            this.newJamToolStripMenuItem.Size = new System.Drawing.Size(122, 22);
            this.newJamToolStripMenuItem.Text = "New Jam";
            this.newJamToolStripMenuItem.Click += new System.EventHandler(this.newJamToolStripMenuItem_Click);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "bullet_green.png");
            this.imageList1.Images.SetKeyName(1, "bullet_orange.png");
            this.imageList1.Images.SetKeyName(2, "bullet_red.png");
            this.imageList1.Images.SetKeyName(3, "monitor.png");
            this.imageList1.Images.SetKeyName(4, "photo.png");
            this.imageList1.Images.SetKeyName(5, "transmit.png");
            this.imageList1.Images.SetKeyName(6, "transmit_delete.png");
            this.imageList1.Images.SetKeyName(7, "transmit_go.png");
            this.imageList1.Images.SetKeyName(8, "error.png");
            this.imageList1.Images.SetKeyName(9, "monitor_never_sent.fw.png");
            this.imageList1.Images.SetKeyName(10, "monitor_not_authed.png");
            this.imageList1.Images.SetKeyName(11, "monitor_not_responding.fw.png");
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(865, 541);
            this.Controls.Add(this.c_JamHierarchy);
            this.DoubleBuffered = true;
            this.IsMdiContainer = true;
            this.Name = "MainForm";
            this.Text = "JamCast Controller";
            this.c_TreeContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView c_JamHierarchy;
        private System.Windows.Forms.ContextMenuStrip c_TreeContextMenu;
        private System.Windows.Forms.ToolStripMenuItem newJamToolStripMenuItem;
        private System.Windows.Forms.ImageList imageList1;
    }
}