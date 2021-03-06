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
            this._googleCloudProjectID = new System.Windows.Forms.TextBox();
            this.c_ControllerSlackAPITokenLabel = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this._macAddressReportingEndpoint = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this._googleCloudStorageSecret = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this._googleCloudOAuthEndpointURL = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.c_ProjectorSlackChannelsLabel = new System.Windows.Forms.Label();
            this._projectorSlackAPIToken = new System.Windows.Forms.TextBox();
            this._projectorSlackChannels = new System.Windows.Forms.TextBox();
            this.c_ClientSlackAPITokenLabel = new System.Windows.Forms.Label();
            this.c_CreateBootstrap = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.m_SlackConnectionStatus = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.c_PackageAndDeploy = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.c_PackageAndDeployNoBootstrap = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this._pubSubOperationsText = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // _googleCloudProjectID
            // 
            this._googleCloudProjectID.Location = new System.Drawing.Point(184, 19);
            this._googleCloudProjectID.Name = "_googleCloudProjectID";
            this._googleCloudProjectID.Size = new System.Drawing.Size(192, 20);
            this._googleCloudProjectID.TabIndex = 0;
            this._googleCloudProjectID.TextChanged += new System.EventHandler(this._googleCloudProjectID_TextChanged);
            // 
            // c_ControllerSlackAPITokenLabel
            // 
            this.c_ControllerSlackAPITokenLabel.AutoSize = true;
            this.c_ControllerSlackAPITokenLabel.Location = new System.Drawing.Point(17, 22);
            this.c_ControllerSlackAPITokenLabel.Name = "c_ControllerSlackAPITokenLabel";
            this.c_ControllerSlackAPITokenLabel.Size = new System.Drawing.Size(124, 13);
            this.c_ControllerSlackAPITokenLabel.TabIndex = 1;
            this.c_ControllerSlackAPITokenLabel.Text = "Google Cloud Project ID:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this._macAddressReportingEndpoint);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this._googleCloudStorageSecret);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this._googleCloudOAuthEndpointURL);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this._googleCloudProjectID);
            this.groupBox1.Controls.Add(this.c_ControllerSlackAPITokenLabel);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(382, 198);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Controller Settings";
            // 
            // _macAddressReportingEndpoint
            // 
            this._macAddressReportingEndpoint.Location = new System.Drawing.Point(184, 71);
            this._macAddressReportingEndpoint.Name = "_macAddressReportingEndpoint";
            this._macAddressReportingEndpoint.Size = new System.Drawing.Size(192, 20);
            this._macAddressReportingEndpoint.TabIndex = 7;
            this._macAddressReportingEndpoint.TextChanged += new System.EventHandler(this._macAddressReportingEndpoint_TextChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(17, 74);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(168, 13);
            this.label7.TabIndex = 8;
            this.label7.Text = "MAC Address Reporting Endpoint:";
            // 
            // _googleCloudStorageSecret
            // 
            this._googleCloudStorageSecret.Location = new System.Drawing.Point(184, 97);
            this._googleCloudStorageSecret.Name = "_googleCloudStorageSecret";
            this._googleCloudStorageSecret.Size = new System.Drawing.Size(192, 20);
            this._googleCloudStorageSecret.TabIndex = 5;
            this._googleCloudStorageSecret.TextChanged += new System.EventHandler(this._googleCloudStorageSecret_TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(17, 100);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(128, 13);
            this.label6.TabIndex = 6;
            this.label6.Text = "Controller Storage Secret:";
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(15, 125);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(359, 61);
            this.label5.TabIndex = 4;
            this.label5.Text = resources.GetString("label5.Text");
            // 
            // _googleCloudOAuthEndpointURL
            // 
            this._googleCloudOAuthEndpointURL.Location = new System.Drawing.Point(184, 45);
            this._googleCloudOAuthEndpointURL.Name = "_googleCloudOAuthEndpointURL";
            this._googleCloudOAuthEndpointURL.Size = new System.Drawing.Size(192, 20);
            this._googleCloudOAuthEndpointURL.TabIndex = 2;
            this._googleCloudOAuthEndpointURL.TextChanged += new System.EventHandler(this._googleCloudOAuthEndpointURL_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(17, 48);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(161, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "OAuth Token Provider Endpoint:";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.c_ProjectorSlackChannelsLabel);
            this.groupBox2.Controls.Add(this._projectorSlackAPIToken);
            this.groupBox2.Controls.Add(this._projectorSlackChannels);
            this.groupBox2.Controls.Add(this.c_ClientSlackAPITokenLabel);
            this.groupBox2.Location = new System.Drawing.Point(12, 282);
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
            // _projectorSlackAPIToken
            // 
            this._projectorSlackAPIToken.Location = new System.Drawing.Point(134, 19);
            this._projectorSlackAPIToken.Name = "_projectorSlackAPIToken";
            this._projectorSlackAPIToken.Size = new System.Drawing.Size(242, 20);
            this._projectorSlackAPIToken.TabIndex = 0;
            this._projectorSlackAPIToken.TextChanged += new System.EventHandler(this._projectorSlackAPIToken_TextChanged);
            // 
            // _projectorSlackChannels
            // 
            this._projectorSlackChannels.Location = new System.Drawing.Point(203, 45);
            this._projectorSlackChannels.Name = "_projectorSlackChannels";
            this._projectorSlackChannels.Size = new System.Drawing.Size(173, 20);
            this._projectorSlackChannels.TabIndex = 2;
            this._projectorSlackChannels.TextChanged += new System.EventHandler(this._projectorSlackChannels_TextChanged);
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
            this.c_CreateBootstrap.Location = new System.Drawing.Point(262, 364);
            this.c_CreateBootstrap.Name = "c_CreateBootstrap";
            this.c_CreateBootstrap.Size = new System.Drawing.Size(124, 70);
            this.c_CreateBootstrap.TabIndex = 7;
            this.c_CreateBootstrap.Text = "Create Bootstrap";
            this.c_CreateBootstrap.UseVisualStyleBackColor = true;
            this.c_CreateBootstrap.Click += new System.EventHandler(this.c_CreateBootstrap_Click);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(12, 366);
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
            this.m_SlackConnectionStatus.Location = new System.Drawing.Point(-2, 600);
            this.m_SlackConnectionStatus.Name = "m_SlackConnectionStatus";
            this.m_SlackConnectionStatus.Size = new System.Drawing.Size(410, 35);
            this.m_SlackConnectionStatus.TabIndex = 9;
            this.m_SlackConnectionStatus.Text = "Not connected to Google Cloud Pub/Sub!";
            this.m_SlackConnectionStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.Location = new System.Drawing.Point(12, 491);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(240, 41);
            this.label2.TabIndex = 11;
            this.label2.Text = "Package and deploy the client and projector software associated with the controll" +
    "er out to all computers.  ";
            // 
            // c_PackageAndDeploy
            // 
            this.c_PackageAndDeploy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.c_PackageAndDeploy.Location = new System.Drawing.Point(262, 489);
            this.c_PackageAndDeploy.Name = "c_PackageAndDeploy";
            this.c_PackageAndDeploy.Size = new System.Drawing.Size(124, 43);
            this.c_PackageAndDeploy.TabIndex = 10;
            this.c_PackageAndDeploy.Text = "Package and Deploy";
            this.c_PackageAndDeploy.UseVisualStyleBackColor = true;
            this.c_PackageAndDeploy.Click += new System.EventHandler(this.c_PackageAndDeploy_Click);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.Location = new System.Drawing.Point(12, 543);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(240, 41);
            this.label3.TabIndex = 13;
            this.label3.Text = "Package and deploy the client and projector software, but skip deployment of the " +
    "bootstrap executable.";
            // 
            // c_PackageAndDeployNoBootstrap
            // 
            this.c_PackageAndDeployNoBootstrap.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.c_PackageAndDeployNoBootstrap.Location = new System.Drawing.Point(262, 541);
            this.c_PackageAndDeployNoBootstrap.Name = "c_PackageAndDeployNoBootstrap";
            this.c_PackageAndDeployNoBootstrap.Size = new System.Drawing.Size(124, 43);
            this.c_PackageAndDeployNoBootstrap.TabIndex = 12;
            this.c_PackageAndDeployNoBootstrap.Text = "Package and Deploy (No Bootstrap)";
            this.c_PackageAndDeployNoBootstrap.UseVisualStyleBackColor = true;
            this.c_PackageAndDeployNoBootstrap.Click += new System.EventHandler(this.c_PackageAndDeployExternal_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this._pubSubOperationsText);
            this.groupBox3.Controls.Add(this.label8);
            this.groupBox3.Location = new System.Drawing.Point(12, 216);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(382, 60);
            this.groupBox3.TabIndex = 7;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Google Cloud Statistics (estimate)";
            // 
            // _pubSubOperationsText
            // 
            this._pubSubOperationsText.Enabled = false;
            this._pubSubOperationsText.Location = new System.Drawing.Point(134, 19);
            this._pubSubOperationsText.Name = "_pubSubOperationsText";
            this._pubSubOperationsText.Size = new System.Drawing.Size(242, 20);
            this._pubSubOperationsText.TabIndex = 0;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(17, 22);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(107, 13);
            this.label8.TabIndex = 1;
            this.label8.Text = "Pub/Sub Operations:";
            // 
            // label9
            // 
            this.label9.Location = new System.Drawing.Point(12, 442);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(240, 41);
            this.label9.TabIndex = 15;
            this.label9.Text = "Clears the entire queue of all bootstrap messages.";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(262, 440);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(124, 43);
            this.button1.TabIndex = 14;
            this.button1.Text = "Clear Entire Message Queue";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // JamEditForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(406, 635);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.c_PackageAndDeployNoBootstrap);
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
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox _googleCloudProjectID;
        private System.Windows.Forms.Label c_ControllerSlackAPITokenLabel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label c_ProjectorSlackChannelsLabel;
        private System.Windows.Forms.TextBox _projectorSlackAPIToken;
        private System.Windows.Forms.TextBox _projectorSlackChannels;
        private System.Windows.Forms.Label c_ClientSlackAPITokenLabel;
        private System.Windows.Forms.Button c_CreateBootstrap;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label m_SlackConnectionStatus;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button c_PackageAndDeploy;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button c_PackageAndDeployNoBootstrap;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox _googleCloudOAuthEndpointURL;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox _googleCloudStorageSecret;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox _pubSubOperationsText;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox _macAddressReportingEndpoint;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button button1;
    }
}