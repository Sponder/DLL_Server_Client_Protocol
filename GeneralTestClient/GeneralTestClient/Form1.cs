using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace GeneralTestClient
{
    public partial class Form1 : Form
    {

        #region Variables
        /// <summary>
        /// client instance
        /// </summary>
        Async_Server_Client2._0.AsyncClient client;

        /// <summary>
        /// the port the client uses to connect to the server
        /// </summary>
        int port = 11000;

        /// <summary>
        /// client connected?
        /// </summary>
        bool clientConnected = false;

        bool feSelected = false;
        bool botSelected = false;

        /// <summary>
        /// clients Ip address in string form
        /// </summary>
        string clientIPAddressString;

        /// <summary>
        /// clients IP address
        /// </summary>
        IPAddress clientIPAddress;

        /// <summary>
        /// List og connected client GUID's
        /// </summary>
        List<Guid> clientList;
        List<Guid> botList;
        Async_Server_Client2._0.bot activeBot;

        /// <summary>
        /// int array to see what label was clicked
        /// 1st int == what listbox (0 = fe client list, 1= bot list)
        /// 2nd int == what item in the listbox
        /// </summary>
        int[] selectedLabel = {-1,-1};
        int oldFeSelected = -1;
        int oldBotSelected = -1;
        int botSensorVal = 50;

        bool ledStatus = true;
        #region delegates
        public delegate void _refresh();
        public delegate void _setClientList(List<Guid> list);
        public delegate void _setBotList(List<Guid> list);
        public delegate void _serverGone();
        public delegate void _setChatMsg(string msg);
        public delegate void _setActiveBot(Async_Server_Client2._0.bot bot);

        #endregion

        #endregion

        #region Constructors
        public Form1()
        {
            InitializeComponent();
            clientList = new List<Guid>();
            clientIPAddressString = GetLocalIpAddress();
            clientIPAddress = IPAddress.Parse(clientIPAddressString);

            ConnectServer();
            aquaGauge1.Enabled = false;
            aquaGauge1.MaxValue = 100;
            aquaGauge1.MinValue = 0;
            RefreshForm();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            this.DoubleBuffered = true;
        }
        #endregion

        #region methods

        public void RefreshForm()
        {
            try
            {
                cbLed.Enabled = false;
                btnSendMsg.Enabled = false;
                btnLockBot.Enabled = false;
                if (client !=null && client.isConnected == true)
                {
                    toolStripClientStatusLabel.Text = "Connected...";


                    lbConnectedClients.Items.Clear();
                    lbConnectedBots.Items.Clear();  

                    if (this.clientList.Count() > 0)
                    {

                        foreach (Guid g in this.clientList)
                        {
                            lbConnectedClients.Items.Add(g.ToString());
                        }

                    }

                    
                    if (this.botList.Count() > 0)
                    {

                        foreach (Guid g in this.botList)
                        {
                            lbConnectedBots.Items.Add(g.ToString());
                        }

                    }

                    if(selectedLabel[0] != -1 && selectedLabel[1] != -1)
                    {
                        if (selectedLabel[0] == 0)
                        {
                            lbConnectedClients.SelectedIndex = selectedLabel[1];
                            btnSendMsg.Enabled = true;
                        }else if(selectedLabel[0] == 1)
                        {
                            lbConnectedBots.SelectedIndex = selectedLabel[1];
                            
                            btnLockBot.Enabled = true;
                            btnSendMsg.Enabled = true;

                            if (this.client.isControlling != Guid.Empty)
                            {
                                cbLed.Enabled = true;
                                this.aquaGauge1.Enabled = true;
                                this.aquaGauge1.Value = this.activeBot.sensorVal;
                                this.aquaGauge1.Update();
                                this.aquaGauge1.Refresh();

                                propertyGridActiveBot.SelectedObject = this.activeBot;

                                if (ledStatus == true)
                                {
                                    cbLed.Checked = true;
                                }
                                else {
                                    cbLed.Checked = false;
                                }

                            }
                        }
                        oldBotSelected = -1;
                        oldFeSelected = -1;
                    }
                }
                else {
                    lbConnectedClients.Items.Clear();
                    toolStripClientStatusLabel.Text = "Disconnected...";
                }
            }
            catch (Exception e)
            { }
        }

        public void UpdateForm()
        {
            if (this.InvokeRequired)
            {
                _refresh d = new _refresh(RefreshForm);
                this.Invoke(d);
            }
            else {
                RefreshForm();
                this.Refresh();
            }
        }


        public void SetClientList(List<Guid> l)
        {
            if (this.InvokeRequired)
            {
                _setClientList d = new _setClientList(setClientList);
                this.Invoke(d, new object[] {l});
            }
            else {
                this.clientList = l;
                RefreshForm();
            }
        }
        private void setClientList(List<Guid> l)
        {
            this.clientList = l;
            RefreshForm();
        }


        public void SetBotList(List<Guid> l)
        {
            if (this.InvokeRequired)
            {
                _setBotList d = new _setBotList(setBotList);
                this.Invoke(d, new object[] { l });
            }
            else
            {
                this.botList = l;
                RefreshForm();
            }
        }
        private void setBotList(List<Guid> l)
        {
            this.botList = l;
            RefreshForm();
        }


        public void ServerDisconnected()
        {
            if (this.InvokeRequired)
            {
                _serverGone d = new _serverGone(serverDisconnected);
                this.Invoke(d);
            }
            else {
                serverDisconnected();
            }
            UpdateForm();
        }
        public void serverDisconnected()
        { 
            //server broke the connection:
            this.client = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        public void SetChatMsg(string msg)
        {
            if (this.InvokeRequired)
            {
                _setChatMsg d = new _setChatMsg(setChatMsg);
                this.Invoke(d, new object[]{msg});
            }
            else {
                setChatMsg(msg);
            }
        }
        public void setChatMsg(string msg)
        {
            rtbChat.Text += msg + "\r\n";
        }


        public void SetBot(Async_Server_Client2._0.bot bot)
        {
            this.activeBot = bot;
            RefreshForm();
        }

        private void ConnectServer()
        {
            client = new Async_Server_Client2._0.AsyncClient(clientIPAddress, port);
            client.UpdateForm += new Async_Server_Client2._0.AsyncClient.RefreshForm(UpdateForm);
            client.setClientList += new Async_Server_Client2._0.AsyncClient.updateList(SetClientList);
            client.setBotList += new Async_Server_Client2._0.AsyncClient.updateBotList(SetBotList);
            client.ServerGone += new Async_Server_Client2._0.AsyncClient.ServerDisconnected(ServerDisconnected);
            client.setChatMsg += new Async_Server_Client2._0.AsyncClient.chatMsg(SetChatMsg);
            client.setBot += new Async_Server_Client2._0.AsyncClient.setClientBot(SetBot);
            client.Connect();
            System.Threading.Thread.Sleep(50);
        }

        private void Reconnect()
        {
            if (client != null)
            {
                client.client.Close();
            }
            ConnectServer();
            RefreshForm();
        }


        public void testProtocol()
        {
            string testString = "Dit is een mooie test string....";
            Async_Server_Client2._0.ServerClientProtocol p = new Async_Server_Client2._0.ServerClientProtocol("100");
            int i = 0;
            while(i < 1000000)
            {
                p.AddParameter(testString);
                i++;
            }
            p.clientGuid = client.clientGuid;
            byte[] test = p.Serialize(p);
            client.WriteP(p);

        }

        /// <summary>
        /// method to get local IP addy, tired of manually setting it :)
        /// </summary>
        public string GetLocalIpAddress()
        {
            IPHostEntry host;
            string localIP = "";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                }
            }
            return localIP;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            testProtocol();
        }

        private void buttonReconnect_Click(object sender, EventArgs e)
        {
            Reconnect();
        }

        private void lbConnectedClients_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbConnectedClients.SelectedIndex != -1)
            {
                if (oldFeSelected != lbConnectedClients.SelectedIndex)
                {
                    lbConnectedBots.ClearSelected();
                    selectedLabel[0] = 0;
                    selectedLabel[1] = lbConnectedClients.SelectedIndex;
                    oldFeSelected = lbConnectedClients.SelectedIndex;
                    RefreshForm();
                }
                else
                {
                    return;
                }
            }
        }

        private void lbConnectedBots_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbConnectedBots.SelectedIndex != -1)
            {
                if (oldBotSelected != lbConnectedBots.SelectedIndex)
                {
                    lbConnectedClients.ClearSelected();
                    selectedLabel[0] = 1;
                    selectedLabel[1] = lbConnectedBots.SelectedIndex;
                    oldBotSelected = lbConnectedBots.SelectedIndex;
                    RefreshForm();
                }
                else
                {
                    return;
                }
            }
        }
        private void btnSendMsg_Click(object sender, EventArgs e)
        {
            if (selectedLabel[0] == 0)
            {
                string msg = tbSendChat.Text;
                Async_Server_Client2._0.ServerClientProtocol p = new Async_Server_Client2._0.ServerClientProtocol("03");
                p.AddParameter(msg);
                p.clientGuid = client.clientGuid;
                p.sendToClient = new Guid(lbConnectedClients.SelectedItem.ToString());
                client.WriteP(p);
            }else if(selectedLabel[0] == 1)
            {
                Async_Server_Client2._0.ServerClientProtocol p = new Async_Server_Client2._0.ServerClientProtocol("06");
                p.clientGuid = this.client.clientGuid;
                p.sendToBot = new Guid(lbConnectedBots.SelectedItem.ToString());
                string msg = tbSendChat.Text;
                p.AddParameter(msg);
                client.WriteP(p);
            }
        }

        private void lbConnectedClients_MouseClick(object sender, MouseEventArgs e)
        {
            if (lbConnectedClients.SelectedItem == null)
            {
                selectedLabel[0] = -1;
                selectedLabel[0] = -1;
                lbConnectedClients.ClearSelected();
            }
        }

        private void lbConnectedBots_MouseClick(object sender, MouseEventArgs e)
        {
            if (lbConnectedBots.SelectedItem == null)
            {
                selectedLabel[0] = -1;
                selectedLabel[0] = -1;
                lbConnectedBots.ClearSelected();
            }
        }

        private void btnLockBot_Click(object sender, EventArgs e)
        {
            //what bot we clicked?
            if (lbConnectedBots != null)
            {
                Guid g = new Guid(lbConnectedBots.SelectedItem.ToString());
                Async_Server_Client2._0.ServerClientProtocol p = new Async_Server_Client2._0.ServerClientProtocol("05");
                p.clientGuid = this.client.clientGuid;
                p.sendToBot = g;

                client.WriteP(p);
            }
                

        }

        #endregion

        private void cbLed_Click(object sender, EventArgs e)
        {
            string cmd;
            if (ledStatus == true)
            {
                cmd = "0000";
                ledStatus = false;
            }
            else {
                cmd = "0001";
                ledStatus = true;
            }

            Async_Server_Client2._0.ServerClientProtocol p = new Async_Server_Client2._0.ServerClientProtocol("06");
            p.AddParameter(cmd);
            p.clientGuid = this.client.clientGuid;
            Guid botG = new Guid(this.lbConnectedBots.SelectedItem.ToString());
            p.sendToBot = botG;
            client.WriteP(p);

        }

    }
}
