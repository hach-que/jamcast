namespace Controller.Forms
{
    partial class ClientEditForm
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
            this.label4 = new System.Windows.Forms.Label();
            this._emailAddress = new System.Windows.Forms.TextBox();
            this._fullName = new System.Windows.Forms.TextBox();
            this.c_ControllerSlackAPITokenLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this._hostname = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this._ipAddresses = new System.Windows.Forms.TextBox();
            this._macAddresses = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this._cloudOperations = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this._close = new System.Windows.Forms.Button();
            this._viewScreen = new System.Windows.Forms.Button();
            this._updateTimer = new System.Windows.Forms.Timer(this.components);
            this._lastTimeControllerSentMessageToBootstrapTextBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this._lastTimeControllerRecievedMessageFromBootstrapTextBox = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this._lastTimeBootstrapSentAMessageTextBox = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this._lastTimeBootstrapRecievedAMessageFromControllerAndAckedItTextBox = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(11, 41);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(82, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "User Full Name:";
            // 
            // _emailAddress
            // 
            this._emailAddress.Enabled = false;
            this._emailAddress.Location = new System.Drawing.Point(141, 12);
            this._emailAddress.Name = "_emailAddress";
            this._emailAddress.Size = new System.Drawing.Size(286, 20);
            this._emailAddress.TabIndex = 0;
            // 
            // _fullName
            // 
            this._fullName.Enabled = false;
            this._fullName.Location = new System.Drawing.Point(141, 38);
            this._fullName.Name = "_fullName";
            this._fullName.Size = new System.Drawing.Size(286, 20);
            this._fullName.TabIndex = 2;
            // 
            // c_ControllerSlackAPITokenLabel
            // 
            this.c_ControllerSlackAPITokenLabel.AutoSize = true;
            this.c_ControllerSlackAPITokenLabel.Location = new System.Drawing.Point(11, 15);
            this.c_ControllerSlackAPITokenLabel.Name = "c_ControllerSlackAPITokenLabel";
            this.c_ControllerSlackAPITokenLabel.Size = new System.Drawing.Size(101, 13);
            this.c_ControllerSlackAPITokenLabel.TabIndex = 1;
            this.c_ControllerSlackAPITokenLabel.Text = "User Email Address:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 67);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(106, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Computer Hostname:";
            // 
            // _hostname
            // 
            this._hostname.Enabled = false;
            this._hostname.Location = new System.Drawing.Point(141, 64);
            this._hostname.Name = "_hostname";
            this._hostname.Size = new System.Drawing.Size(286, 20);
            this._hostname.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 204);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(120, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Computer IP Addresses:";
            // 
            // _ipAddresses
            // 
            this._ipAddresses.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._ipAddresses.Enabled = false;
            this._ipAddresses.Location = new System.Drawing.Point(13, 227);
            this._ipAddresses.Multiline = true;
            this._ipAddresses.Name = "_ipAddresses";
            this._ipAddresses.Size = new System.Drawing.Size(413, 110);
            this._ipAddresses.TabIndex = 8;
            // 
            // _macAddresses
            // 
            this._macAddresses.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._macAddresses.Enabled = false;
            this._macAddresses.Location = new System.Drawing.Point(14, 371);
            this._macAddresses.Multiline = true;
            this._macAddresses.Name = "_macAddresses";
            this._macAddresses.Size = new System.Drawing.Size(413, 110);
            this._macAddresses.TabIndex = 10;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 348);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(133, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Computer MAC Addresses:";
            // 
            // _cloudOperations
            // 
            this._cloudOperations.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._cloudOperations.Enabled = false;
            this._cloudOperations.Location = new System.Drawing.Point(141, 487);
            this._cloudOperations.Name = "_cloudOperations";
            this._cloudOperations.Size = new System.Drawing.Size(286, 20);
            this._cloudOperations.TabIndex = 11;
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(11, 490);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(91, 13);
            this.label5.TabIndex = 12;
            this.label5.Text = "Cloud Operations:";
            // 
            // _close
            // 
            this._close.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._close.Location = new System.Drawing.Point(12, 518);
            this._close.Name = "_close";
            this._close.Size = new System.Drawing.Size(189, 42);
            this._close.TabIndex = 13;
            this._close.Text = "Close";
            this._close.UseVisualStyleBackColor = true;
            this._close.Click += new System.EventHandler(this._close_Click);
            // 
            // _viewScreen
            // 
            this._viewScreen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._viewScreen.Enabled = false;
            this._viewScreen.Location = new System.Drawing.Point(237, 518);
            this._viewScreen.Name = "_viewScreen";
            this._viewScreen.Size = new System.Drawing.Size(189, 42);
            this._viewScreen.TabIndex = 14;
            this._viewScreen.Text = "View Screen";
            this._viewScreen.UseVisualStyleBackColor = true;
            // 
            // _updateTimer
            // 
            this._updateTimer.Enabled = true;
            this._updateTimer.Tick += new System.EventHandler(this._updateTimer_Tick);
            // 
            // _lastTimeControllerSentMessageToBootstrapTextBox
            // 
            this._lastTimeControllerSentMessageToBootstrapTextBox.Enabled = false;
            this._lastTimeControllerSentMessageToBootstrapTextBox.Location = new System.Drawing.Point(287, 90);
            this._lastTimeControllerSentMessageToBootstrapTextBox.Name = "_lastTimeControllerSentMessageToBootstrapTextBox";
            this._lastTimeControllerSentMessageToBootstrapTextBox.Size = new System.Drawing.Size(139, 20);
            this._lastTimeControllerSentMessageToBootstrapTextBox.TabIndex = 15;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(10, 93);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(234, 13);
            this.label6.TabIndex = 16;
            this.label6.Text = "Last time controller sent a message to bootstrap:";
            // 
            // _lastTimeControllerRecievedMessageFromBootstrapTextBox
            // 
            this._lastTimeControllerRecievedMessageFromBootstrapTextBox.Enabled = false;
            this._lastTimeControllerRecievedMessageFromBootstrapTextBox.Location = new System.Drawing.Point(287, 116);
            this._lastTimeControllerRecievedMessageFromBootstrapTextBox.Name = "_lastTimeControllerRecievedMessageFromBootstrapTextBox";
            this._lastTimeControllerRecievedMessageFromBootstrapTextBox.Size = new System.Drawing.Size(140, 20);
            this._lastTimeControllerRecievedMessageFromBootstrapTextBox.TabIndex = 17;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(11, 119);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(266, 13);
            this.label7.TabIndex = 18;
            this.label7.Text = "Last time controller recieved a message from bootstrap:";
            // 
            // _lastTimeBootstrapSentAMessageTextBox
            // 
            this._lastTimeBootstrapSentAMessageTextBox.Enabled = false;
            this._lastTimeBootstrapSentAMessageTextBox.Location = new System.Drawing.Point(286, 142);
            this._lastTimeBootstrapSentAMessageTextBox.Name = "_lastTimeBootstrapSentAMessageTextBox";
            this._lastTimeBootstrapSentAMessageTextBox.Size = new System.Drawing.Size(140, 20);
            this._lastTimeBootstrapSentAMessageTextBox.TabIndex = 19;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(10, 145);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(176, 13);
            this.label8.TabIndex = 20;
            this.label8.Text = "Last time bootstrap sent a message:";
            // 
            // _lastTimeBootstrapRecievedAMessageFromControllerAndAckedItTextBox
            // 
            this._lastTimeBootstrapRecievedAMessageFromControllerAndAckedItTextBox.Enabled = false;
            this._lastTimeBootstrapRecievedAMessageFromControllerAndAckedItTextBox.Location = new System.Drawing.Point(287, 168);
            this._lastTimeBootstrapRecievedAMessageFromControllerAndAckedItTextBox.Name = "_lastTimeBootstrapRecievedAMessageFromControllerAndAckedItTextBox";
            this._lastTimeBootstrapRecievedAMessageFromControllerAndAckedItTextBox.Size = new System.Drawing.Size(140, 20);
            this._lastTimeBootstrapRecievedAMessageFromControllerAndAckedItTextBox.TabIndex = 21;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(11, 171);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(255, 13);
            this.label9.TabIndex = 22;
            this.label9.Text = "Last time bootstrap acked a message from controller:";
            // 
            // ClientEditForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(438, 570);
            this.Controls.Add(this._lastTimeBootstrapRecievedAMessageFromControllerAndAckedItTextBox);
            this.Controls.Add(this.label9);
            this.Controls.Add(this._lastTimeBootstrapSentAMessageTextBox);
            this.Controls.Add(this.label8);
            this.Controls.Add(this._lastTimeControllerRecievedMessageFromBootstrapTextBox);
            this.Controls.Add(this.label7);
            this.Controls.Add(this._lastTimeControllerSentMessageToBootstrapTextBox);
            this.Controls.Add(this.label6);
            this.Controls.Add(this._viewScreen);
            this.Controls.Add(this._close);
            this.Controls.Add(this._cloudOperations);
            this.Controls.Add(this.label5);
            this.Controls.Add(this._macAddresses);
            this.Controls.Add(this.label3);
            this.Controls.Add(this._ipAddresses);
            this.Controls.Add(this.label2);
            this.Controls.Add(this._hostname);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.c_ControllerSlackAPITokenLabel);
            this.Controls.Add(this._fullName);
            this.Controls.Add(this._emailAddress);
            this.Controls.Add(this.label4);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "ClientEditForm";
            this.Text = "ClientEditForm";
            this.Load += new System.EventHandler(this.ClientEditForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox _emailAddress;
        private System.Windows.Forms.TextBox _fullName;
        private System.Windows.Forms.TextBox _hostname;
        private System.Windows.Forms.TextBox _ipAddresses;
        private System.Windows.Forms.TextBox _macAddresses;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label c_ControllerSlackAPITokenLabel;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox _cloudOperations;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button _close;
        private System.Windows.Forms.Button _viewScreen;
        private System.Windows.Forms.Timer _updateTimer;
        private System.Windows.Forms.TextBox _lastTimeControllerSentMessageToBootstrapTextBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox _lastTimeControllerRecievedMessageFromBootstrapTextBox;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox _lastTimeBootstrapSentAMessageTextBox;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox _lastTimeBootstrapRecievedAMessageFromControllerAndAckedItTextBox;
        private System.Windows.Forms.Label label9;
    }
}