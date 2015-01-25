namespace Controller.Forms
{
    partial class ProjectorEditForm
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
            this.c_TwitterConsumerKey = new System.Windows.Forms.TextBox();
            this.c_ControllerSlackAPITokenLabel = new System.Windows.Forms.Label();
            this.c_TwitterConsumerSecret = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.c_TwitterOAuthToken = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.c_TwitterOAuthSecret = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.c_TwitterSearchQuery = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.c_ProjectorName = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.c_IsPrimaryProjector = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // c_TwitterConsumerKey
            // 
            this.c_TwitterConsumerKey.Location = new System.Drawing.Point(141, 12);
            this.c_TwitterConsumerKey.Name = "c_TwitterConsumerKey";
            this.c_TwitterConsumerKey.Size = new System.Drawing.Size(253, 20);
            this.c_TwitterConsumerKey.TabIndex = 0;
            // 
            // c_ControllerSlackAPITokenLabel
            // 
            this.c_ControllerSlackAPITokenLabel.AutoSize = true;
            this.c_ControllerSlackAPITokenLabel.Location = new System.Drawing.Point(11, 15);
            this.c_ControllerSlackAPITokenLabel.Name = "c_ControllerSlackAPITokenLabel";
            this.c_ControllerSlackAPITokenLabel.Size = new System.Drawing.Size(113, 13);
            this.c_ControllerSlackAPITokenLabel.TabIndex = 1;
            this.c_ControllerSlackAPITokenLabel.Text = "Twitter Consumer Key:";
            // 
            // c_TwitterConsumerSecret
            // 
            this.c_TwitterConsumerSecret.Location = new System.Drawing.Point(141, 38);
            this.c_TwitterConsumerSecret.Name = "c_TwitterConsumerSecret";
            this.c_TwitterConsumerSecret.Size = new System.Drawing.Size(253, 20);
            this.c_TwitterConsumerSecret.TabIndex = 2;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(11, 41);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(126, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Twitter Consumer Secret:";
            // 
            // c_TwitterOAuthToken
            // 
            this.c_TwitterOAuthToken.Location = new System.Drawing.Point(141, 64);
            this.c_TwitterOAuthToken.Name = "c_TwitterOAuthToken";
            this.c_TwitterOAuthToken.Size = new System.Drawing.Size(253, 20);
            this.c_TwitterOAuthToken.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 67);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(109, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Twitter OAuth Token:";
            // 
            // c_TwitterOAuthSecret
            // 
            this.c_TwitterOAuthSecret.Location = new System.Drawing.Point(141, 90);
            this.c_TwitterOAuthSecret.Name = "c_TwitterOAuthSecret";
            this.c_TwitterOAuthSecret.Size = new System.Drawing.Size(253, 20);
            this.c_TwitterOAuthSecret.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 93);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(109, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Twitter OAuth Secret:";
            // 
            // c_TwitterSearchQuery
            // 
            this.c_TwitterSearchQuery.Location = new System.Drawing.Point(141, 116);
            this.c_TwitterSearchQuery.Name = "c_TwitterSearchQuery";
            this.c_TwitterSearchQuery.Size = new System.Drawing.Size(253, 20);
            this.c_TwitterSearchQuery.TabIndex = 8;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(11, 119);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(110, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Twitter Search Query:";
            // 
            // c_ProjectorName
            // 
            this.c_ProjectorName.Location = new System.Drawing.Point(141, 142);
            this.c_ProjectorName.Name = "c_ProjectorName";
            this.c_ProjectorName.Size = new System.Drawing.Size(253, 20);
            this.c_ProjectorName.TabIndex = 10;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(11, 145);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(83, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "Projector Name:";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(275, 228);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(119, 30);
            this.button1.TabIndex = 12;
            this.button1.Text = "Save and Update";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.SettingsUpdated);
            // 
            // c_IsPrimaryProjector
            // 
            this.c_IsPrimaryProjector.AutoSize = true;
            this.c_IsPrimaryProjector.Location = new System.Drawing.Point(15, 179);
            this.c_IsPrimaryProjector.Name = "c_IsPrimaryProjector";
            this.c_IsPrimaryProjector.Size = new System.Drawing.Size(105, 17);
            this.c_IsPrimaryProjector.TabIndex = 13;
            this.c_IsPrimaryProjector.Text = "Primary Projector";
            this.c_IsPrimaryProjector.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(12, 201);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(236, 60);
            this.label6.TabIndex = 14;
            this.label6.Text = "The primary projector responds to user commands sent to the \"jamcast\" user in Sla" +
    "ck.  You should have exactly one projector designated as the primary projector.";
            // 
            // ProjectorEditForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(406, 270);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.c_IsPrimaryProjector);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.c_ProjectorName);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.c_TwitterSearchQuery);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.c_TwitterOAuthSecret);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.c_TwitterOAuthToken);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.c_ControllerSlackAPITokenLabel);
            this.Controls.Add(this.c_TwitterConsumerSecret);
            this.Controls.Add(this.c_TwitterConsumerKey);
            this.Controls.Add(this.label4);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "ProjectorEditForm";
            this.Text = "ProjectorEditForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.JamEditForm_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox c_TwitterConsumerKey;
        private System.Windows.Forms.Label c_ControllerSlackAPITokenLabel;
        private System.Windows.Forms.TextBox c_TwitterConsumerSecret;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox c_TwitterOAuthToken;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox c_TwitterOAuthSecret;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox c_TwitterSearchQuery;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox c_ProjectorName;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.CheckBox c_IsPrimaryProjector;
        private System.Windows.Forms.Label label6;

    }
}