﻿namespace Controller.Forms
{
    partial class JamEditForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(JamEditForm));
            this.c_ControllerSlackAPIToken = new System.Windows.Forms.TextBox();
            this.c_ControllerSlackAPITokenLabel = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.c_ProjectorSlackChannelsLabel = new System.Windows.Forms.Label();
            this.c_ClientSlackAPIToken = new System.Windows.Forms.TextBox();
            this.c_ProjectorSlackChannels = new System.Windows.Forms.TextBox();
            this.c_ClientSlackAPITokenLabel = new System.Windows.Forms.Label();
            this.c_CreateBootstrap = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.m_SlackConnectionStatus = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.c_PackageAndDeploy = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.c_PackageAndDeployExternal = new System.Windows.Forms.Button();
            this.c_ControllerStorageAPIToken = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.c_StorageTokenLink = new System.Windows.Forms.LinkLabel();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // c_ControllerSlackAPIToken
            // 
            this.c_ControllerSlackAPIToken.Location = new System.Drawing.Point(134, 19);
            this.c_ControllerSlackAPIToken.Name = "c_ControllerSlackAPIToken";
            this.c_ControllerSlackAPIToken.Size = new System.Drawing.Size(242, 20);
            this.c_ControllerSlackAPIToken.TabIndex = 0;
            this.c_ControllerSlackAPIToken.TextChanged += new System.EventHandler(this.c_SlackAPIToken_TextChanged);
            // 
            // c_ControllerSlackAPITokenLabel
            // 
            this.c_ControllerSlackAPITokenLabel.AutoSize = true;
            this.c_ControllerSlackAPITokenLabel.Location = new System.Drawing.Point(17, 22);
            this.c_ControllerSlackAPITokenLabel.Name = "c_ControllerSlackAPITokenLabel";
            this.c_ControllerSlackAPITokenLabel.Size = new System.Drawing.Size(91, 13);
            this.c_ControllerSlackAPITokenLabel.TabIndex = 1;
            this.c_ControllerSlackAPITokenLabel.Text = "Slack API Token:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.c_StorageTokenLink);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.c_ControllerStorageAPIToken);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.c_ControllerSlackAPIToken);
            this.groupBox1.Controls.Add(this.c_ControllerSlackAPITokenLabel);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(382, 141);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Controller Settings";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.c_ProjectorSlackChannelsLabel);
            this.groupBox2.Controls.Add(this.c_ClientSlackAPIToken);
            this.groupBox2.Controls.Add(this.c_ProjectorSlackChannels);
            this.groupBox2.Controls.Add(this.c_ClientSlackAPITokenLabel);
            this.groupBox2.Location = new System.Drawing.Point(12, 159);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(382, 76);
            this.groupBox2.TabIndex = 6;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Client && Projector Settings";
            // 
            // c_ProjectorSlackChannelsLabel
            // 
            this.c_ProjectorSlackChannelsLabel.AutoSize = true;
            this.c_ProjectorSlackChannelsLabel.Location = new System.Drawing.Point(17, 48);
            this.c_ProjectorSlackChannelsLabel.Name = "c_ProjectorSlackChannelsLabel";
            this.c_ProjectorSlackChannelsLabel.Size = new System.Drawing.Size(177, 13);
            this.c_ProjectorSlackChannelsLabel.TabIndex = 3;
            this.c_ProjectorSlackChannelsLabel.Text = "Slack Channels (comma-seperated):";
            // 
            // c_ClientSlackAPIToken
            // 
            this.c_ClientSlackAPIToken.Location = new System.Drawing.Point(134, 19);
            this.c_ClientSlackAPIToken.Name = "c_ClientSlackAPIToken";
            this.c_ClientSlackAPIToken.Size = new System.Drawing.Size(242, 20);
            this.c_ClientSlackAPIToken.TabIndex = 0;
            this.c_ClientSlackAPIToken.TextChanged += new System.EventHandler(this.c_ClientSlackAPIToken_TextChanged);
            // 
            // c_ProjectorSlackChannels
            // 
            this.c_ProjectorSlackChannels.Location = new System.Drawing.Point(203, 45);
            this.c_ProjectorSlackChannels.Name = "c_ProjectorSlackChannels";
            this.c_ProjectorSlackChannels.Size = new System.Drawing.Size(173, 20);
            this.c_ProjectorSlackChannels.TabIndex = 2;
            this.c_ProjectorSlackChannels.TextChanged += new System.EventHandler(this.c_ProjectorSlackChannels_TextChanged);
            // 
            // c_ClientSlackAPITokenLabel
            // 
            this.c_ClientSlackAPITokenLabel.AutoSize = true;
            this.c_ClientSlackAPITokenLabel.Location = new System.Drawing.Point(17, 22);
            this.c_ClientSlackAPITokenLabel.Name = "c_ClientSlackAPITokenLabel";
            this.c_ClientSlackAPITokenLabel.Size = new System.Drawing.Size(91, 13);
            this.c_ClientSlackAPITokenLabel.TabIndex = 1;
            this.c_ClientSlackAPITokenLabel.Text = "Slack API Token:";
            // 
            // c_CreateBootstrap
            // 
            this.c_CreateBootstrap.Location = new System.Drawing.Point(264, 241);
            this.c_CreateBootstrap.Name = "c_CreateBootstrap";
            this.c_CreateBootstrap.Size = new System.Drawing.Size(124, 70);
            this.c_CreateBootstrap.TabIndex = 7;
            this.c_CreateBootstrap.Text = "Create Bootstrap";
            this.c_CreateBootstrap.UseVisualStyleBackColor = true;
            this.c_CreateBootstrap.Click += new System.EventHandler(this.c_CreateBootstrap_Click);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(14, 243);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(240, 68);
            this.label1.TabIndex = 8;
            this.label1.Text = resources.GetString("label1.Text");
            // 
            // m_SlackConnectionStatus
            // 
            this.m_SlackConnectionStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.m_SlackConnectionStatus.BackColor = System.Drawing.Color.LightCoral;
            this.m_SlackConnectionStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_SlackConnectionStatus.Location = new System.Drawing.Point(-2, 427);
            this.m_SlackConnectionStatus.Name = "m_SlackConnectionStatus";
            this.m_SlackConnectionStatus.Size = new System.Drawing.Size(410, 35);
            this.m_SlackConnectionStatus.TabIndex = 9;
            this.m_SlackConnectionStatus.Text = "Not connected to Slack!";
            this.m_SlackConnectionStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(14, 321);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(240, 41);
            this.label2.TabIndex = 11;
            this.label2.Text = "Package and deploy the client and projector software associated with the controll" +
    "er out to all computers.  ";
            // 
            // c_PackageAndDeploy
            // 
            this.c_PackageAndDeploy.Location = new System.Drawing.Point(264, 319);
            this.c_PackageAndDeploy.Name = "c_PackageAndDeploy";
            this.c_PackageAndDeploy.Size = new System.Drawing.Size(124, 43);
            this.c_PackageAndDeploy.TabIndex = 10;
            this.c_PackageAndDeploy.Text = "Package and Deploy";
            this.c_PackageAndDeploy.UseVisualStyleBackColor = true;
            this.c_PackageAndDeploy.Click += new System.EventHandler(this.c_PackageAndDeploy_Click);
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(14, 373);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(240, 41);
            this.label3.TabIndex = 13;
            this.label3.Text = "Package and deploy the client and projector software from another folder on this " +
    "computer.  Useful during development of JamCast.";
            // 
            // c_PackageAndDeployExternal
            // 
            this.c_PackageAndDeployExternal.Location = new System.Drawing.Point(264, 371);
            this.c_PackageAndDeployExternal.Name = "c_PackageAndDeployExternal";
            this.c_PackageAndDeployExternal.Size = new System.Drawing.Size(124, 43);
            this.c_PackageAndDeployExternal.TabIndex = 12;
            this.c_PackageAndDeployExternal.Text = "Package and Deploy External";
            this.c_PackageAndDeployExternal.UseVisualStyleBackColor = true;
            this.c_PackageAndDeployExternal.Click += new System.EventHandler(this.c_PackageAndDeployExternal_Click);
            // 
            // c_ControllerStorageAPIToken
            // 
            this.c_ControllerStorageAPIToken.Location = new System.Drawing.Point(134, 45);
            this.c_ControllerStorageAPIToken.Name = "c_ControllerStorageAPIToken";
            this.c_ControllerStorageAPIToken.Size = new System.Drawing.Size(242, 20);
            this.c_ControllerStorageAPIToken.TabIndex = 2;
            this.c_ControllerStorageAPIToken.TextChanged += new System.EventHandler(this.c_ControllerStorageAPIToken_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(17, 48);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(111, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Slack Storage Token:";
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(17, 73);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(359, 45);
            this.label5.TabIndex = 4;
            this.label5.Text = "The slack API token should be the API token for \"jamcast-controller\".  The slack " +
    "storage token needs to be a real user token (such as your own account).  Get a s" +
    "torage token from:";
            // 
            // c_StorageTokenLink
            // 
            this.c_StorageTokenLink.AutoSize = true;
            this.c_StorageTokenLink.Location = new System.Drawing.Point(17, 118);
            this.c_StorageTokenLink.Name = "c_StorageTokenLink";
            this.c_StorageTokenLink.Size = new System.Drawing.Size(133, 13);
            this.c_StorageTokenLink.TabIndex = 5;
            this.c_StorageTokenLink.TabStop = true;
            this.c_StorageTokenLink.Text = "https://api.slack.com/web";
            this.c_StorageTokenLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.c_StorageTokenLink_LinkClicked);
            // 
            // JamEditForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(406, 462);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.c_PackageAndDeployExternal);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.c_PackageAndDeploy);
            this.Controls.Add(this.m_SlackConnectionStatus);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.c_CreateBootstrap);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "JamEditForm";
            this.Text = "JamEditForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.JamEditForm_FormClosing);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox c_ControllerSlackAPIToken;
        private System.Windows.Forms.Label c_ControllerSlackAPITokenLabel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label c_ProjectorSlackChannelsLabel;
        private System.Windows.Forms.TextBox c_ClientSlackAPIToken;
        private System.Windows.Forms.TextBox c_ProjectorSlackChannels;
        private System.Windows.Forms.Label c_ClientSlackAPITokenLabel;
        private System.Windows.Forms.Button c_CreateBootstrap;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label m_SlackConnectionStatus;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button c_PackageAndDeploy;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button c_PackageAndDeployExternal;
        private System.Windows.Forms.LinkLabel c_StorageTokenLink;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox c_ControllerStorageAPIToken;
        private System.Windows.Forms.Label label4;

    }
}