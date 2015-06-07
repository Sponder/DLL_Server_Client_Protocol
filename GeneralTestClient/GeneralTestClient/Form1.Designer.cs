namespace GeneralTestClient
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripClientStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.button1 = new System.Windows.Forms.Button();
            this.buttonReconnect = new System.Windows.Forms.Button();
            this.lbConnectedClients = new System.Windows.Forms.ListBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cbLed = new System.Windows.Forms.CheckBox();
            this.btnLockBot = new System.Windows.Forms.Button();
            this.labelConnectionStatus = new System.Windows.Forms.Label();
            this.lbConnectedBots = new System.Windows.Forms.ListBox();
            this.rtbChat = new System.Windows.Forms.RichTextBox();
            this.tbSendChat = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.propertyGridActiveBot = new System.Windows.Forms.PropertyGrid();
            this.aquaGauge1 = new AquaControls.AquaGauge();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.btnSendMsg = new System.Windows.Forms.Button();
            this.klok1 = new UserControls.Klok();
            this.statusStrip1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripClientStatusLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 499);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1456, 22);
            this.statusStrip1.TabIndex = 0;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripClientStatusLabel
            // 
            this.toolStripClientStatusLabel.Name = "toolStripClientStatusLabel";
            this.toolStripClientStatusLabel.Size = new System.Drawing.Size(118, 17);
            this.toolStripClientStatusLabel.Text = "toolStripStatusLabel1";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(6, 41);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(124, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // buttonReconnect
            // 
            this.buttonReconnect.Location = new System.Drawing.Point(6, 71);
            this.buttonReconnect.Name = "buttonReconnect";
            this.buttonReconnect.Size = new System.Drawing.Size(124, 23);
            this.buttonReconnect.TabIndex = 2;
            this.buttonReconnect.Text = "Reconnect";
            this.buttonReconnect.UseVisualStyleBackColor = true;
            this.buttonReconnect.Click += new System.EventHandler(this.buttonReconnect_Click);
            // 
            // lbConnectedClients
            // 
            this.lbConnectedClients.FormattingEnabled = true;
            this.lbConnectedClients.Location = new System.Drawing.Point(6, 100);
            this.lbConnectedClients.Name = "lbConnectedClients";
            this.lbConnectedClients.Size = new System.Drawing.Size(124, 134);
            this.lbConnectedClients.TabIndex = 3;
            this.lbConnectedClients.MouseClick += new System.Windows.Forms.MouseEventHandler(this.lbConnectedClients_MouseClick);
            this.lbConnectedClients.SelectedIndexChanged += new System.EventHandler(this.lbConnectedClients_SelectedIndexChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox1.Controls.Add(this.cbLed);
            this.groupBox1.Controls.Add(this.btnLockBot);
            this.groupBox1.Controls.Add(this.labelConnectionStatus);
            this.groupBox1.Controls.Add(this.lbConnectedBots);
            this.groupBox1.Controls.Add(this.lbConnectedClients);
            this.groupBox1.Controls.Add(this.buttonReconnect);
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(136, 484);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Connection";
            // 
            // cbLed
            // 
            this.cbLed.AutoSize = true;
            this.cbLed.Location = new System.Drawing.Point(10, 459);
            this.cbLed.Name = "cbLed";
            this.cbLed.Size = new System.Drawing.Size(40, 17);
            this.cbLed.TabIndex = 9;
            this.cbLed.Text = "led";
            this.cbLed.UseVisualStyleBackColor = true;
            this.cbLed.Click += new System.EventHandler(this.cbLed_Click);
            // 
            // btnLockBot
            // 
            this.btnLockBot.Location = new System.Drawing.Point(10, 391);
            this.btnLockBot.Name = "btnLockBot";
            this.btnLockBot.Size = new System.Drawing.Size(120, 23);
            this.btnLockBot.TabIndex = 4;
            this.btnLockBot.Text = "Lock";
            this.btnLockBot.UseVisualStyleBackColor = true;
            this.btnLockBot.Click += new System.EventHandler(this.btnLockBot_Click);
            // 
            // labelConnectionStatus
            // 
            this.labelConnectionStatus.AutoSize = true;
            this.labelConnectionStatus.Location = new System.Drawing.Point(7, 22);
            this.labelConnectionStatus.Name = "labelConnectionStatus";
            this.labelConnectionStatus.Size = new System.Drawing.Size(35, 13);
            this.labelConnectionStatus.TabIndex = 3;
            this.labelConnectionStatus.Text = "label1";
            // 
            // lbConnectedBots
            // 
            this.lbConnectedBots.FormattingEnabled = true;
            this.lbConnectedBots.Location = new System.Drawing.Point(6, 240);
            this.lbConnectedBots.Name = "lbConnectedBots";
            this.lbConnectedBots.Size = new System.Drawing.Size(124, 134);
            this.lbConnectedBots.TabIndex = 3;
            this.lbConnectedBots.MouseClick += new System.Windows.Forms.MouseEventHandler(this.lbConnectedBots_MouseClick);
            this.lbConnectedBots.SelectedIndexChanged += new System.EventHandler(this.lbConnectedBots_SelectedIndexChanged);
            // 
            // rtbChat
            // 
            this.rtbChat.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.rtbChat.Location = new System.Drawing.Point(6, 15);
            this.rtbChat.Name = "rtbChat";
            this.rtbChat.Size = new System.Drawing.Size(549, 406);
            this.rtbChat.TabIndex = 5;
            this.rtbChat.Text = "";
            // 
            // tbSendChat
            // 
            this.tbSendChat.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tbSendChat.Location = new System.Drawing.Point(6, 427);
            this.tbSendChat.Name = "tbSendChat";
            this.tbSendChat.Size = new System.Drawing.Size(549, 20);
            this.tbSendChat.TabIndex = 6;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.klok1);
            this.groupBox2.Controls.Add(this.propertyGridActiveBot);
            this.groupBox2.Controls.Add(this.aquaGauge1);
            this.groupBox2.Controls.Add(this.textBox1);
            this.groupBox2.Controls.Add(this.btnSendMsg);
            this.groupBox2.Controls.Add(this.tbSendChat);
            this.groupBox2.Controls.Add(this.rtbChat);
            this.groupBox2.Location = new System.Drawing.Point(154, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(1290, 484);
            this.groupBox2.TabIndex = 7;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Chat";
            // 
            // propertyGridActiveBot
            // 
            this.propertyGridActiveBot.Location = new System.Drawing.Point(992, 15);
            this.propertyGridActiveBot.Name = "propertyGridActiveBot";
            this.propertyGridActiveBot.Size = new System.Drawing.Size(209, 460);
            this.propertyGridActiveBot.TabIndex = 10;
            // 
            // aquaGauge1
            // 
            this.aquaGauge1.BackColor = System.Drawing.Color.Transparent;
            this.aquaGauge1.DialColor = System.Drawing.Color.Lavender;
            this.aquaGauge1.DialText = null;
            this.aquaGauge1.Glossiness = 11.36364F;
            this.aquaGauge1.Location = new System.Drawing.Point(571, 168);
            this.aquaGauge1.MaxValue = 0F;
            this.aquaGauge1.MinValue = 0F;
            this.aquaGauge1.Name = "aquaGauge1";
            this.aquaGauge1.RecommendedValue = 0F;
            this.aquaGauge1.Size = new System.Drawing.Size(150, 150);
            this.aquaGauge1.TabIndex = 9;
            this.aquaGauge1.ThresholdPercent = 0F;
            this.aquaGauge1.Value = 0F;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(561, 15);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(415, 460);
            this.textBox1.TabIndex = 8;
            this.textBox1.Text = resources.GetString("textBox1.Text");
            // 
            // btnSendMsg
            // 
            this.btnSendMsg.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSendMsg.Location = new System.Drawing.Point(480, 453);
            this.btnSendMsg.Name = "btnSendMsg";
            this.btnSendMsg.Size = new System.Drawing.Size(75, 23);
            this.btnSendMsg.TabIndex = 7;
            this.btnSendMsg.Text = "Send!";
            this.btnSendMsg.UseVisualStyleBackColor = true;
            this.btnSendMsg.Click += new System.EventHandler(this.btnSendMsg_Click);
            // 
            // klok1
            // 
            this.klok1.beginDegree = 110D;
            this.klok1.currentVal = 0D;
            this.klok1.endDegree = 70D;
            this.klok1.Location = new System.Drawing.Point(727, 181);
            this.klok1.maxValue = 405D;
            this.klok1.maxValueSmall = 100D;
            this.klok1.minValue = 135D;
            this.klok1.minValueSmall = 0D;
            this.klok1.Name = "klok1";
            this.klok1.needleVal = 0;
            this.klok1.nrScalePoints = 8;
            this.klok1.Size = new System.Drawing.Size(250, 250);
            this.klok1.TabIndex = 11;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1456, 521);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.statusStrip1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripClientStatusLabel;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button buttonReconnect;
        private System.Windows.Forms.ListBox lbConnectedClients;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label labelConnectionStatus;
        private System.Windows.Forms.RichTextBox rtbChat;
        private System.Windows.Forms.TextBox tbSendChat;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnSendMsg;
        private System.Windows.Forms.ListBox lbConnectedBots;
        private System.Windows.Forms.Button btnLockBot;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.CheckBox cbLed;
        private AquaControls.AquaGauge aquaGauge1;
        private System.Windows.Forms.PropertyGrid propertyGridActiveBot;
        private UserControls.Klok klok1;
    }
}

