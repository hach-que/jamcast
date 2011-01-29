namespace Client
{
    partial class WhoAreYou
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WhoAreYou));
            this.c_EnterYourNameLabel = new System.Windows.Forms.Label();
            this.c_NameTextBox = new System.Windows.Forms.TextBox();
            this.c_GoButton = new System.Windows.Forms.Button();
            this.c_CancelButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // c_EnterYourNameLabel
            // 
            this.c_EnterYourNameLabel.AutoSize = true;
            this.c_EnterYourNameLabel.Location = new System.Drawing.Point(9, 9);
            this.c_EnterYourNameLabel.Name = "c_EnterYourNameLabel";
            this.c_EnterYourNameLabel.Size = new System.Drawing.Size(103, 13);
            this.c_EnterYourNameLabel.TabIndex = 0;
            this.c_EnterYourNameLabel.Text = "Enter your full name:";
            // 
            // c_NameTextBox
            // 
            this.c_NameTextBox.Location = new System.Drawing.Point(12, 30);
            this.c_NameTextBox.Name = "c_NameTextBox";
            this.c_NameTextBox.Size = new System.Drawing.Size(212, 20);
            this.c_NameTextBox.TabIndex = 1;
            this.c_NameTextBox.TextChanged += new System.EventHandler(this.c_NameTextBox_TextChanged);
            // 
            // c_GoButton
            // 
            this.c_GoButton.Enabled = false;
            this.c_GoButton.Location = new System.Drawing.Point(130, 56);
            this.c_GoButton.Name = "c_GoButton";
            this.c_GoButton.Size = new System.Drawing.Size(94, 27);
            this.c_GoButton.TabIndex = 2;
            this.c_GoButton.Text = "Go!";
            this.c_GoButton.UseVisualStyleBackColor = true;
            this.c_GoButton.Click += new System.EventHandler(this.c_GoButton_Click);
            // 
            // c_CancelButton
            // 
            this.c_CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.c_CancelButton.Location = new System.Drawing.Point(12, 56);
            this.c_CancelButton.Name = "c_CancelButton";
            this.c_CancelButton.Size = new System.Drawing.Size(94, 27);
            this.c_CancelButton.TabIndex = 3;
            this.c_CancelButton.Text = "Cancel";
            this.c_CancelButton.UseVisualStyleBackColor = true;
            this.c_CancelButton.Click += new System.EventHandler(this.c_CancelButton_Click);
            // 
            // WhoAreYou
            // 
            this.AcceptButton = this.c_GoButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.c_CancelButton;
            this.ClientSize = new System.Drawing.Size(236, 95);
            this.Controls.Add(this.c_CancelButton);
            this.Controls.Add(this.c_GoButton);
            this.Controls.Add(this.c_NameTextBox);
            this.Controls.Add(this.c_EnterYourNameLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Who Are You?";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label c_EnterYourNameLabel;
        private System.Windows.Forms.TextBox c_NameTextBox;
        private System.Windows.Forms.Button c_GoButton;
        private System.Windows.Forms.Button c_CancelButton;
    }
}