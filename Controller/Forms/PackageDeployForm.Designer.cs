namespace Controller.Forms
{
    partial class PackageDeployForm
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
            this.c_ProgressBar = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // c_ProgressBar
            // 
            this.c_ProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.c_ProgressBar.Location = new System.Drawing.Point(12, 12);
            this.c_ProgressBar.Name = "c_ProgressBar";
            this.c_ProgressBar.Size = new System.Drawing.Size(388, 22);
            this.c_ProgressBar.TabIndex = 0;
            // 
            // PackageDeployForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(412, 45);
            this.Controls.Add(this.c_ProgressBar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "PackageDeployForm";
            this.Text = "Deploy package";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ProgressBar c_ProgressBar;
    }
}