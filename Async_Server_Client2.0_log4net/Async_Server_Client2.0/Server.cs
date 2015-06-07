using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using log4net;

namespace Async_Server_Client2._0
{
    public class Server
    {
        #region Variables
        /// <summary>
        /// default length for client read buffer
        /// </summary>
        private const int DefaultClientReadBufferLength = 4096;
        private TcpListener tcpListener;
        public bool serverRunning = false;
        public Encoding encoding { get; set; }
        public Guid serverGuid { get; set; }
        public string localIpAddress { get; set; }
        public int nrConnectedClients { get; set; }
        public bool isRgbServer { get; set; }
        public List<AsyncClient> clients { get; set; }
        public List<bot> bots { get; set; }
        public List<Guid> frontEndList { get; set; }
        public List<Guid> botList { get; set; }

        public delegate void RefreshForm();
        public event RefreshForm UpdateForm;

        public delegate void UpdateRtbCallback(string data);
        public UpdateRtbCallback AddToLog;

        private static readonly ILog logger = LogManager.GetLogger(typeof(Server));

        #endregion

        #region events

        /// <summary>
        /// Occurs when client connects to server
        /// </summary>
        public event EventHandler Connected;

        /// <summary>
        /// Occurs when client disconnects from server
        /// </summary>
        public event EventHandler Disconnected;

        /// <summary>
        /// Occurs when data is read by client
        /// </summary>
        public event EventHandler<DataReadEventArgs> DataRead;

        /// <summary>
        /// Occurs when data is written by client
        /// </summary>
        public event EventHandler<DataWrittenEventArgs> DataWritten;

        /// <summary>
        /// Occurs when exception is thrown during connection
        /// </summary>
        public event EventHandler<ExceptionEventArgs> ClientConnectException;

        /// <summary>
        /// Occurs when exception is thrown while writing data
        /// </summary>
        public event EventHandler<ExceptionEventArgs> ClientWriteException;

        /// <summary>
        /// Occurs when exception is thrown while reading data
        /// </summary>
        public event EventHandler<ExceptionEventArgs> ClientReadException;

        #endregion

        #region constructors
        /// <summary>
        /// private constructor for all thingies default:
        /// </summary>
        private Server()
        {
            this.encoding = Encoding.Default;
            nrConnectedClients = 0;
            this.clients = new List<AsyncClient>();
            log4net.Config.XmlConfigurator.Configure();
        }

        /// <summary>
        /// constructor based upon an ipaddress
        /// </summary>
        /// <param name="localAddr"></param>
        /// <param name="port"></param>
        public Server(IPAddress localAddr, int port)
            : this()
        {
            tcpListener = new TcpListener(localAddr, port);
        }


        /// <summary>
        /// constructor based upon an IPEndpoint
        /// </summary>
        /// <param name="localEP"></param>
        public Server(IPEndPoint localEP)
            : this()
        {
            tcpListener = new TcpListener(localEP);
        }

        #endregion

        #region Methods


        public void ClientDisconnects(AsyncClient client)
        {
            client.client.GetStream().Close();
            client.client.Close();
        }


        public AsyncClient FindBotInListByGuid(Guid g)
        {
            try {
                AsyncClient bot = bots.Find(b => b.clientGuid == g);
                
                    if(bot != null)
                    {
                        return bot;
                    }
                    return null;
                }
                catch(Exception e)
                {
                return null;
                }
        }

        public AsyncClient FindClientInListByGuid(Guid g)
        {
            try
            {
                AsyncClient c = clients.Find(client => client.clientGuid == g);
                if (c != null)
                {
                    return c;
                }
                    return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// finds a connected client in the list
        /// </summary>
        public AsyncClient FindClientInList(ServerClientProtocol p)
        {
            try
            {
                Guid g = p.clientGuid;
                if (g != null)
                {
                    AsyncClient clientFound = clients.Find(client => client.clientGuid == g);
                    if (clientFound != null)
                    {
                        return clientFound;
                    }

                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        /// <summary>
        /// Parse the protocol in p
        /// </summary>
        /// <param name="p"></param>
        public void ParseServerClientProtocol(ServerClientProtocol p)
        {
            try
            {
                if(p.clientGuid != Guid.Empty)
                {
                    AsyncClient requestingClient = FindClientInList(p);
                    int clientId = this.clients.IndexOf(requestingClient);
                    switch (p.command)
                    { 
                        case "00":
                            //client confirmed last action
                            AddToLog("client: " + p.clientGuid  + " confirmed his last action.");
                            break;
                        case "01":
                            //client ACKs his Guid, update our list
                            Guid g = p.clientGuid;
                            AsyncClient clientFound = clients.Find(client => client.clientGuid == g);
                            clients[clientId].guidConfirmed = true;
                            logger.Info("client: " + p.clientGuid.ToString() + " accepted his Guid.");
                            UpdateClientLists();
                            
                            break;
                        case "02":
                            break;
                        case "03":
                            //client chat:
                            Guid clientGuid = p.clientGuid;
                            Guid receiverGuid = p.sendToClient;
                            string msg = p.Parameters[0];

                            ServerClientProtocol packet = new ServerClientProtocol("03");
                            packet.AddParameter(msg);
                            byte[] packetSerialized = packet.Serialize(packet);
                            Write(receiverGuid, packetSerialized);
                            logger.Info("frontEnd client: " + clientGuid.ToString() + " send a message to: " + receiverGuid.ToString());
                            AddToLog("client: " + clientGuid.ToString() + " wrote a message to: " + receiverGuid.ToString());

                            break;

                        case "05":
                                //client wants to lock a bot:
                            Guid feGuid = p.clientGuid; // front end client
                            Guid botGuid = p.sendToBot;
                            bot bot = this.bots.Find(b => b.clientGuid == botGuid);
                            int botRecordnr = this.bots.FindIndex(b => b.clientGuid == botGuid);
                            if (bot.ControlledBy == Guid.Empty)
                            {
                                //lege controlled by, locken die hap!
                                lock (this.bots)
                                {
                                    this.bots[botRecordnr].ControlledBy = feGuid;
                                }
                                
                                ServerClientProtocol r = new ServerClientProtocol("04");
                                r.botGuid = botGuid;
                                r.clientGuid = feGuid;
                                WriteP(r);
                                AddToLog("client: " + feGuid.ToString() + " locked bot: " + botGuid.ToString());
                                logger.Info("frontEnd client: " + feGuid.ToString() + " locked bot: " + botGuid.ToString());
                                UpdateClientLists();

                                //iedereen weet dat bot gelocked is, stuur bot naar client:
                                byte[] botA = Serialize(bot);

                                FrameMsg(botA); 
                                //Write(requestingClient.client, botA);
                            }
                            else { 
                                //bot is al controlled...
                            }

                            break;

                        case "06":
                            //fe wants to send something to a client:

                            //first check if fe is controlling the bot:
                            botGuid = p.sendToBot;
                            feGuid = p.clientGuid;
                            AsyncClient sendToBot = this.bots.Find(b => b.ControlledBy == feGuid);
                            if (sendToBot != null)
                            {
                               WriteToBot(sendToBot.client, p.Parameters[0]);
                            }
                            logger.Info("frontEnd " + feGuid.ToString() + " send command: " + p.Parameters[0] + " to bot: " + botGuid.ToString());
                            break;
                        case "11":

                            //bot responded, update the clients with the new bot.
                            Guid controllingFE = p.bot.ControlledBy;
                            WriteP(p);

                            break;
                        case "99":
                            //RgbController/sim disconnected
                            ClientDisconnects(requestingClient);
                            lock (this.clients)
                            {
                                clients.Remove(requestingClient);
                            }
                            //UpdateClientRgbList();
                            break;
                        case "100":
                            AddToLog("Received lotsa bytes!");
                            break;
                    }
                }
            }
            catch (Exception ex)
            { }
        }

        bool CheckIfBotIsLocked(Guid g)
        {
            //find the bot:
            AsyncClient bot = FindBotInListByGuid(g);
            if (bot != null)
            {
                if(bot.clientGuid == Guid.Empty)
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// update the bots (Async clients list)
        /// </summary>
        /// <param name="list"></param>
        public void UpdateBotList(List<bot> list)
        {
            this.bots = list;
            UpdateClientLists();
        } 

        public void UpdateClientLists()
        {
            ServerClientProtocol ucl = new ServerClientProtocol("02");
            if (clients != null && clients.Count > 0)
            {
                foreach (AsyncClient c in clients)
                {
                    ucl.ClientList.Add(c.clientGuid);
                }
            }
            if (bots != null && bots.Count > 0)
            {
                foreach (AsyncClient c in bots)
                {
                    ucl.BotList.Add(c.clientGuid);
                }
            }
            byte[] uclArray = ucl.Serialize(ucl);
            Write(uclArray);
            AddToLog("...Done Updateting lists..");
        }

        public byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        /// <summary>
        /// method to tell the server to start listening for clients
        /// </summary>
            public bool StartServer() 
            {
                try
                {
                    this.tcpListener.Start();
                    this.tcpListener.BeginAcceptTcpClient(AcceptTcpClientCallback, null);
                    this.serverRunning = true;
                    this.serverGuid = Guid.NewGuid();
                    logger.Info("Server started");
                    //AddToLog("Server Started");
                    //UpdateForm();
                    return true;
                }
                catch (Exception e)
                {
                    return false;
                }
            }

            /// <summary>
        /// method to stop the server
        /// </summary>
    
            public bool StopServer()
            {
                foreach(AsyncClient c in this.clients)
                {
                    c.Disconnect();
                }
                clients.Clear();
                this.tcpListener.Stop();
                this.serverGuid = Guid.Empty;
                this.serverRunning = false;
                //AddToLog("Server Stopped");
                nrConnectedClients = 0;
                return false;
                //UpdateForm();
            }


            public void WriteP(ServerClientProtocol p)
            {
                byte[] cmd = p.Serialize(p);
                Write(p.clientGuid, cmd);
            }

            /// <summary>
            /// Write byte array to a bot
            /// </summary>
            /// <param name="tcpClient">TcpClient</param>
            /// <param name="data">string data</param>
            public void WriteToBot(TcpClient tcpClient, string data)
            {
                byte[] bytes = this.encoding.GetBytes(data);
                byte[] msg = Async_Server_Client2._0.BotServer.FrameMsg(bytes);
                NetworkStream ns = tcpClient.GetStream();
                ns.BeginWrite(msg, 0, msg.Length, WriteCallback, tcpClient);
            }

            /// <summary>
            /// method to write a string to a tcpClient
            /// </summary>
            /// <param name="tcpClient"></param>
            /// <param name="data"></param>

            public void Write(TcpClient tcpClient, string data)
            {
                byte[] bytes = this.encoding.GetBytes(data);
                Write(tcpClient, bytes);
            }

            /// <summary>
            /// write a string to all connected clients
            /// </summary>
            /// <param name="data"></param>
            public void Write(string data)
            {
                foreach (AsyncClient client in this.clients)
                    Write(client.client, data);
            }

            /// <summary>
            /// write a byte array to all connected clients
            /// </summary>
            /// <param name="bytes"></param>
            public void Write(byte[] bytes)
            {
                foreach (AsyncClient client in this.clients)
                    Write(client.client, bytes);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="g"></param>
            /// <param name="bytes"></param>
            public void Write(Guid g, byte[] bytes)
            { 
                AsyncClient c = FindClientInListByGuid(g);
                TcpClient tc = c.client;
                Write(tc, bytes);
            }

            /// <summary>
            /// write a byte array to a Tcp Client
            /// </summary>
            /// <param name="tcpClient"></param>
            /// <param name="bytes"></param>
            public void Write(TcpClient tcpClient, byte[] bytes)
            {
                NetworkStream ns = tcpClient.GetStream();
                bytes = FrameMsg(bytes);
                ns.BeginWrite(bytes, 0, bytes.Length, WriteCallback, tcpClient);
            }

            /// <summary>
            /// get the address of the client
            /// </summary>
            /// <param name="client"></param>
            /// <returns></returns>
            public IPAddress clientAddress(AsyncClient client)
            {
                Socket s = client.client.Client;
                IPEndPoint remoteEndPoint = s.RemoteEndPoint as IPEndPoint;
                IPAddress ip = remoteEndPoint.Address;
                return ip;
            }

            /// <summary>
            /// frame byte[]
            /// </summary>
            /// <param name="msg">the byte array to send</param>
            /// <returns>the byte array with framing</returns>
            private byte[] FrameMsg(byte[] msg)
            {
                try
                {
                    byte[] msgLengthBytes = BitConverter.GetBytes(msg.Length);
                    byte[] data = new byte[msg.Length + 4];
                    Buffer.BlockCopy(msgLengthBytes, 0, data, 0, 4);
                    Buffer.BlockCopy(msg, 0, data, 4, msg.Length);

                    return data;
                }
                catch (Exception e)
                {
                    return null;
                }
            }

            /// <summary>
            /// deframes a byte array
            /// </summary>
            /// <param name="msg">the byte array to deframe</param>
            /// <returns>the deframed byte array</returns>
            private byte[] DeFrameMsg(byte[] msg)
            {
                try
                {
                    //messageFraming:
                    byte[] lengthBufferBytes = new byte[4];
                    //copy first 4 bytes from buffer to the lengthBuffer
                    Buffer.BlockCopy(msg, 0, lengthBufferBytes, 0, 4);
                    /*
                    // If the system architecture is little-endian (that is, little end first), 
                    // reverse the byte array. 
                    if (BitConverter.IsLittleEndian)
                    Array.Reverse(bytes);

                    int i = BitConverter.ToInt32(bytes, 0);
                    Console.WriteLine("int: {0}", i);
                    */
                    //if (BitConverter.IsLittleEndian)
                    //    Array.Reverse(lengthBufferBytes);

                    int lengthBuffer = BitConverter.ToInt32(lengthBufferBytes, 0);
                    byte[] data = new byte[lengthBuffer];
                    Buffer.BlockCopy(msg, 4, data, 0, lengthBuffer);

                    return data;

                }
                catch (Exception ex)
                {
                    return null;
                }

            }

            /// <summary>
            /// Serialize an object
            /// </summary>
            /// <param name="o"></param>
            /// <returns></returns>
            public byte[] Serialize(object o)
            {
                try
                {
                    if (o != null)
                    {
                        byte[] cmdSerialized;
                        MemoryStream ms = new MemoryStream();
                        BinaryFormatter bf = new BinaryFormatter();
                        bf.Serialize(ms, o);
                        cmdSerialized = ms.ToArray();
                        ms.Close();
                        return cmdSerialized;
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    return null;
                }
            }

            public void SetClientGuid(AsyncClient client)
            {
                ServerClientProtocol p = new ServerClientProtocol("01");
                p.clientGuid = client.clientGuid;
                byte[] cmd = p.Serialize(p);
                Write(client.client, cmd);
            }

            public void RemoveClient(Guid g)
            {

                //AsyncClient clientFound = clients.Find(client => client.clientGuid == g);


                AsyncClient c = this.clients.Find(client => client.clientGuid == g);
                if (c != null)
                {
                    
                    AddToLog("Client: " + c.clientGuid.ToString() + " disconnected...");
                    //check if client has control over any bot:
                    if (this.bots != null)
                    {
                        if (this.bots.Find(b => b.ControlledBy == c.clientGuid) != null)
                        {
                            int botRecordnr = this.bots.FindIndex(b => b.ControlledBy == c.clientGuid);
                            this.bots[botRecordnr].ControlledBy = Guid.Empty;
                            logger.Info("bot: " + this.bots[botRecordnr].clientGuid.ToString() + " was released from frontend: " + c.clientGuid.ToString());
                        }
                    }

                    lock (this.clients)
                    {
                        clients.Remove(c);
                    }
                    logger.Info("client: " + c.clientGuid.ToString() + " disconnected.");
                    UpdateClientLists();
                }
                //if found, delete it!
            }
            
            /// <summary>
            /// gets the nr of bytes of a msg
            /// </summary>
            /// <param name="msg">byte array containing the message</param>
            /// <returns>int: nr of bytes</returns>
            private int GetMsgSize(byte[] msg)
            {
                try
                {
                    byte[] lengthBufferBytes = new byte[4];
                    Buffer.BlockCopy(msg, 0, lengthBufferBytes, 0, 4);
                    int lengthBuffer = BitConverter.ToInt32(lengthBufferBytes, 0);
                    return lengthBuffer;
                }
                catch (Exception ex)
                {
                    return 0;
                }

            }
        #endregion

        #region Callbacks
            /// <summary>
            /// callback for the accept tcp client method.
            /// </summary>
            /// <param name="result"></param>
            private void AcceptTcpClientCallback(IAsyncResult result)
            {
                try
                {
                    //create a new TCP client:
                    TcpClient tcpClient = tcpListener.EndAcceptTcpClient(result);

                    //create client buffer:
                    byte[] buffer = new byte[DefaultClientReadBufferLength];

                    AsyncClient client = new AsyncClient(tcpClient, buffer);
                    Guid newClientGuid = Guid.NewGuid();
                    client.clientGuid = newClientGuid;
                    
                    lock (this.clients)
                    {
                        this.clients.Add(client);
                    }

                    //get the network stream:
                    NetworkStream ns = client.client.GetStream();
                    //begin reading on the network stream from this client:
                    ns.BeginRead(client.clientBuffer, 0, client.clientBuffer.Length, ReadCallback, client);
                    client.isConnected = true;
                    //AddToLog("Server accepted a connection");
                    //RequestClientGuid(client);
                    SetClientGuid(client);
                    //begin accepting new clients:
                    tcpListener.BeginAcceptTcpClient(AcceptTcpClientCallback, null);
                    UpdateForm();
                    logger.Info("client: " + client.clientGuid.ToString() + " connected.");
                    UpdateClientLists();
                }
                catch (ObjectDisposedException e)
                { }
            }

            /// <summary>
            /// callback for the write operation
            /// </summary>
            /// <param name="asyncResult"></param>
            private void WriteCallback(IAsyncResult asyncResult)
            {
                TcpClient tcpClient = asyncResult.AsyncState as TcpClient;
                NetworkStream ns = tcpClient.GetStream();
                ns.EndWrite(asyncResult);
            }

            /// <summary>
            /// callback for the read operation 
            /// </summary>
            /// <param name="asyncResult"></param>
            private void ReadCallback(IAsyncResult asyncResult)
            {
                try
                {
                    if (asyncResult.AsyncState != null)
                    {
                        AsyncClient client = asyncResult.AsyncState as AsyncClient;
                        if (client == null)
                        {
                            return;
                        }
                        NetworkStream ns = client.client.GetStream();
                        int read = ns.EndRead(asyncResult); 
                        byte[] data = new byte[read];
                        int nrBytesToRead = 0;
                        int bytesReadSoFar = 0;
                        //byte[] buffer = asyncResult.AsyncState as byte[];
                        byte[] buffer = client.clientBuffer;
                        byte[] fullData;

                        //als read == 0, heeft client de verbinding verbroken.
                        if (read == 0)
                        {
                            RemoveClient(client.clientGuid);
                            //lock (this.clients)
                            //{
                            //    this.clients.Remove(client);
                            //}
                            //UpdateClientLists();
                        }

                        if (buffer != null)
                        {
                            Buffer.BlockCopy(buffer, 0, data, 0, read);
                            if (this.DataRead != null)
                                this.DataRead(this, new DataReadEventArgs(data));
                        }

                        nrBytesToRead = GetMsgSize(data) + 4;

                        fullData = new byte[nrBytesToRead];

                        if (nrBytesToRead <= read)
                        {
                            Buffer.BlockCopy(data, 0, fullData, 0, nrBytesToRead);
                        }
                        else {
                            Buffer.BlockCopy(data, 0, fullData, 0, read);
                            bytesReadSoFar += read;

                            while (nrBytesToRead > bytesReadSoFar)
                            {
                                int bytesLeft = nrBytesToRead - bytesReadSoFar;
                                int _read = 0;

                                if (bytesLeft < DefaultClientReadBufferLength)
                                {
                                    _read = ns.Read(buffer, 0, bytesLeft);
                                    Buffer.BlockCopy(buffer, 0, fullData, bytesReadSoFar, bytesLeft);
                                    bytesReadSoFar += _read;
                                }
                                else {
                                    _read = ns.Read(buffer, 0, DefaultClientReadBufferLength);
                                    Buffer.BlockCopy(buffer, 0, fullData, bytesReadSoFar, _read);
                                    bytesReadSoFar += _read;
                                }
                            }
                        }



                        //deframe msg
                        fullData = DeFrameMsg(fullData);

                        //update the log
                        AddToLog("received: " + fullData.GetLength(0).ToString() + " bytes from client: " + client.clientGuid.ToString());


                        if (this.DataRead != null)
                            this.DataRead(this, new DataReadEventArgs(data));

                        object tmpObject = ServerClientProtocol.Deserialize(fullData);
                        if (tmpObject.GetType() == typeof(ServerClientProtocol))
                        {
                            ServerClientProtocol p = (ServerClientProtocol)tmpObject;
                            ParseServerClientProtocol(p);
                        }
                        //hier iets met de data doen..
                        ns.BeginRead(client.clientBuffer, 0, client.clientBuffer.Length, ReadCallback, client);
                    }
                }
                catch (Exception e)
                {
                    //als client != null, maar tcpClient wel, dan is er niet netjes afgesloten, weggooien!
                    if (asyncResult != null)
                    {
                        AsyncClient client = asyncResult.AsyncState as AsyncClient;
                        Guid lostClient = client.clientGuid;
                        RemoveClient(client.clientGuid);
                        //lock (this.clients)
                        //{
                        //    clients.Remove(client);
                        //}
                        //UpdateClientLists();
                        //AddToLog("client with handle: " + client.clientGuid.ToString() + " disconnected...");
                        UpdateForm();

                    }
                }
            }
            #endregion
    }
}
