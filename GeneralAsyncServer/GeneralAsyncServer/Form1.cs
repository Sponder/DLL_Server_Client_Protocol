#region libraries
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

#endregion

#region GeneralAsyncServer
namespace GeneralAsyncServer
{
    public partial class Form1 : Form
    {
        #region Variables 
        
        /// <summary>
        /// server instance
        /// </summary>
        Async_Server_Client2._0.Server server;
        
        /// <summary>
        /// server instance
        /// </summary>
        Async_Server_Client2._0.BotServer botServer;

        /// <summary>
        /// constant: port on which the server listens for new connections
        /// </summary>
        const int serverPort = 11000;

        /// <summary>
        /// constant: port on which the server listens for new connections
        /// </summary>
        const int botServerPort = 11001;

        /// <summary>
        /// bool if server is running or not
        /// </summary>
        bool serverStarted = false;

        /// <summary>
        /// bool if server is running or not
        /// </summary>
        bool botServerStarted = false;
        DateTime timeServerStarted = DateTime.Now;

        string serverIpString;
        IPAddress serverIp;

        #endregion

        #region delegates

        /// <summary>
        /// delegate to itself:
        /// </summary>
        public delegate void _refresh();
        public delegate void _NewBot();
        public delegate void _AddToLog(string msg);

        #endregion

        #region constructors

        /// <summary>
        /// default constructor
        /// </summary>
        public Form1()
        {
            InitializeComponent();
            log4net.Config.XmlConfigurator.Configure(); 
            serverIpString = GetLocalIpAddress();
            serverIp = IPAddress.Parse(serverIpString);
            StartServer();
            RefreshForm();
        }
        #endregion

        #region Methods
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


        private void AddToLog_(string msg)
        {
            if (this.InvokeRequired)
            {
                _AddToLog d = new _AddToLog(AddToLog);
                this.Invoke(d, new object[] { msg });
            }
            else {
                AddToLog(msg);
            }
        }
        
        /// <summary>
        /// adds to rich text box
        /// </summary>
        /// <param name="msg"></param>
        private void AddToLog(string msg)
        {
            richTextBoxLog.AppendText(msg + "\n");
            richTextBoxLog.AppendText("-----------------\r\n");
        }

        public void NewBot()
        {
            if (this.InvokeRequired)
            {
                _NewBot d = new _NewBot(SetServerBotList);
                this.Invoke(d);
            }
            else {
                SetServerBotList();
            }
        }
        /// <summary>
        /// get all botGuids
        /// </summary>
        /// <returns></returns>
        public void SetServerBotList()
        {
            server.bots = botServer.clients;
            server.UpdateClientLists();
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



        /// <summary>
        /// Method to refresh the Form
        /// </summary>
        public void RefreshForm()
        {
            propertyGrid1.SelectedObject = server;
            int selected = lbConnectedClients.SelectedIndex;
            lbConnectedClients.Items.Clear();
            lbConnectedBots.Items.Clear();
            if (serverStarted == true)
            {

                //check if there are any connected clients:
                if (server.clients != null && server.clients.Count() > 0)
                { 
                    foreach(Async_Server_Client2._0.AsyncClient c in server.clients)
                    {
                        lbConnectedClients.Items.Add(c.clientGuid.ToString());
                    }
                }

                //show selected client:

                if (lbConnectedClients.Items.Count > 0)
                {
                    if (selected != -1)
                    {
                        propertyGrid1.SelectedObject = server.clients[selected];
                        lbConnectedClients.SelectedIndex = selected;
                    }
                }

                //check if there are any connected bots:
                if (botServer.clients != null && botServer.clients.Count() > 0)
                {
                    foreach (Async_Server_Client2._0.AsyncClient c in botServer.clients)
                    {
                        lbConnectedBots.Items.Add(c.clientGuid.ToString());
                        
                    }
                }

                //show selected client:

                if (lbConnectedBots.Items.Count > 0)
                {
                    if (selected != -1)
                    {
                        //propertyGrid1.SelectedObject = server.clients[selected];
                        lbConnectedBots.SelectedIndex = selected;
                    }
                }

                ToolStripLabelServerStatus.Text = "Server started at " + timeServerStarted.ToString() +" Server Guid: "
                                                    + server.serverGuid.ToString();

                startToolStripMenuItem.Enabled = false;
                stopToolStripMenuItem.Enabled = true;

            }
            else {
                ToolStripLabelServerStatus.Text = "Server stopped";
                startToolStripMenuItem.Enabled = true;
                stopToolStripMenuItem.Enabled = false;
            }
        }

        /// <summary>
        /// menu bar exits the program
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        /// <summary>
        /// menu bar stops the server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            serverStarted = server.StopServer();
            this.server = null;
            RefreshForm();
        }

        /// <summary>
        /// menu bar starts the server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartServer();
            RefreshForm();
        }

        private void StartServer()
        {
            server = new Async_Server_Client2._0.Server(serverIp, serverPort);
            server.UpdateForm += new Async_Server_Client2._0.Server.RefreshForm(UpdateForm);
            server.AddToLog += new Async_Server_Client2._0.Server.UpdateRtbCallback(AddToLog_);
            serverStarted = server.StartServer();

            //botserver
            botServer = new Async_Server_Client2._0.BotServer(serverIp, botServerPort);
            botServer.UpdateForm += new Async_Server_Client2._0.BotServer.RefreshForm(UpdateForm);
            botServer.AddToLog += new Async_Server_Client2._0.BotServer.UpdateRtbCallback(AddToLog_);
            botServer.setBotList += new Async_Server_Client2._0.BotServer.SetBotList(NewBot);
            botServer.handleBotResponse += new Async_Server_Client2._0.BotServer.HandleBotResponse(HandleBotResponse);
            botServerStarted = botServer.StartServer();

        }

        public void HandleBotResponse(Async_Server_Client2._0.ServerClientProtocol p)
        {
            try {
                /*
                //first find out which bot responded:
                if (server.bots.Find(b => b.clientGuid ==  p.bot.clientGuid) != null)
                {
                    Async_Server_Client2._0.bot bot = server.bots.Find(b => b.clientGuid == p.bot.clientGuid);
                    //check if / who is controlling bot
                    if (server.FindClientInListByGuid(bot.ControlledBy) != null)
                    {
                        Async_Server_Client2._0.AsyncClient c = server.FindClientInListByGuid(bot.ControlledBy);
                        Async_Server_Client2._0.ServerClientProtocol clientUpdate = new Async_Server_Client2._0.ServerClientProtocol("11");
                        p.clientGuid = c.clientGuid;
                        clientUpdate.clientGuid = bot.ControlledBy;
                        server.WriteP(clientUpdate);
                    }
                }
                */
                if (server.FindClientInListByGuid(p.bot.ControlledBy) != null)
                {
                    Async_Server_Client2._0.AsyncClient c = server.FindClientInListByGuid(p.bot.ControlledBy);
                    Async_Server_Client2._0.ServerClientProtocol clientUpdate = new Async_Server_Client2._0.ServerClientProtocol("11");
                    p.clientGuid = c.clientGuid;
                }
                server.ParseServerClientProtocol(p);
            }
            catch (Exception e)
            { }
        }


        #endregion

        private void lbConnectedClients_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(lbConnectedClients.SelectedIndex != -1)
            {
                propertyGrid1.SelectedObject = server.clients[lbConnectedClients.SelectedIndex];
            }
        }

    }
} 

#endregion