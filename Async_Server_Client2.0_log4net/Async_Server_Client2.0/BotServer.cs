using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Async_Server_Client2._0
{
    //<?http://%www.codeproject.com/Articles/140911/log-net-Tutorial?>
    public class BotServer
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
        public List<bot> clients { get; set; }
        public delegate void RefreshForm();
        public event RefreshForm UpdateForm;

        public delegate void UpdateRtbCallback(string data);
        public UpdateRtbCallback AddToLog;

        public delegate void SetBotList();
        public SetBotList setBotList;

        public delegate void HandleBotResponse(ServerClientProtocol p);
        public HandleBotResponse handleBotResponse;

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
        private BotServer()
        {
            this.encoding = Encoding.Default;
            nrConnectedClients = 0;
            this.clients = new List<bot>();
        }

        /// <summary>
        /// constructor based upon an ipaddress
        /// </summary>
        /// <param name="localAddr"></param>
        /// <param name="port"></param>
        public BotServer(IPAddress localAddr, int port)
            : this()
        {
            tcpListener = new TcpListener(localAddr, port);
        }


        /// <summary>
        /// constructor based upon an IPEndpoint
        /// </summary>
        /// <param name="localEP"></param>
        public BotServer(IPEndPoint localEP)
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
        public bot FindClientInList(ServerClientProtocol p)
        {
            try
            {
                Guid g = p.clientGuid;
                if (g != null)
                {
                    bot clientFound = clients.Find(client => client.clientGuid == g);
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


        public void WriteP(ServerClientProtocol p)
        {
            byte[] cmd = p.Serialize(p);
            Write(cmd);
        }

        /// <summary>
        /// parse what the bot has send us, and report to controlling fe.
        /// </summary>
        public void ParseBotCmd(byte[] command, Guid botGuid)
        {
            ServerClientProtocol p = new ServerClientProtocol("11");
            //deframe botcommand
            int nrbytes = command.Count() - 3;
            byte[] cmd = new byte[nrbytes];
            Buffer.BlockCopy(command, 1, cmd, 0, nrbytes);
            string dataString = GetString(cmd);
            //get the bot from the list, so we know who to inform.
            bot bot = this.clients.Find(b => b.clientGuid == botGuid);
            if (bot.ControlledBy != Guid.Empty)
            {
                p.bot = bot;
            }else{
                p.bot = bot;
            }
            //get type of command:
            byte[] cmdA = new byte[2];
            Buffer.BlockCopy(cmd, 0, cmdA, 0, 2);
            string cmdType = Encoding.ASCII.GetString(cmdA);

            /*
             all commands are in hex code
             * 1st 2 are the type of command
             * depending on type, parameters can be added
             * 
             * command types:
             * 
             *      00      Report type         Bot Reports on something (requested or not)
             *                                  1st parameter is type of report
             *                                  2nd parameter is the value
             *      01      Request type        Bot wants something                              
             *                                  1st parameter is type of request
             * 
             */

            switch (cmdType)
            {
                case "00":

                    break;
                case "01":
                    byte[] reportType = new byte[2];
                    byte[] value = new byte[2];

                    Buffer.BlockCopy(command, 3, reportType, 0, 2);
                    Buffer.BlockCopy(command, 5, value, 0, 2);
                    string reportTypeS = Encoding.ASCII.GetString(reportType);
                    string valueS = Encoding.ASCII.GetString(value);
                    int val = Int32.Parse(valueS);
                    //now we got the type of Report:
                    switch (reportTypeS)
                    { 
                        case "00": // led status
                            if (val == 0)
                            {
                                bot.led = false;
                                AddToLog("Led off!");
                            }
                            else if (val == 1)
                            {
                                bot.led = true;
                                AddToLog("Led on!");
                            }

                            break;
                        case "01": //sensorval
                            bot.sensorVal = val;
                            break;
                    }
                    break;

            }
            p.bot = bot;
            handleBotResponse(p);
        }


        /// <summary>
        /// Parse the protocol in p
        /// </summary>
        /// <param name="p"></param>
        public void ParseServerClientProtocol(ServerClientProtocol p)
        {
            try
            {
                if (p.clientGuid != Guid.Empty)
                {
                    bot requestingClient = FindClientInList(p);
                    int clientId = this.clients.IndexOf(requestingClient);
                    switch (p.command)
                    {
                        case "00":
                            //client confirmed last action
                            AddToLog("client: " + p.clientGuid + " confirmed his last action.");
                            break;
                        case "01":
                            //client ACKs his Guid, update our list
                            Guid g = p.clientGuid;
                            AsyncClient clientFound = clients.Find(client => client.clientGuid == g);
                            clients[clientId].guidConfirmed = true;
                            setBotList();

                            break;
                        case "02":
                            //
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

                            AddToLog("client: " + clientGuid.ToString() + " wrote a message to: " + receiverGuid.ToString());

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


        public byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public string GetString(byte[] bytes)
        {
            return System.Text.Encoding.ASCII.GetString (bytes); 
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
            foreach (AsyncClient c in this.clients)
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
        /// returns 5 bytes: [startchar][msgLengthBytes]
        /// </summary>
        /// <param name="msg">the byte array to send</param>
        /// <returns>the byte array with framing</returns>
        public static byte[] FrameMsg(byte[] msg)
        {
            try
            {
                byte[] msgLengthBytes = BitConverter.GetBytes(msg.Length);
                byte[] data = new byte[msg.Length + 3];
                char startChar = '%';
                byte startCharA = Convert.ToByte(startChar);
                byte[] sg = new byte[1];
                sg[0] = startCharA;
                Buffer.BlockCopy(sg, 0, data, 0, 1);
                Buffer.BlockCopy(msgLengthBytes, 0, data, 1, 2);
                Buffer.BlockCopy(msg, 0, data, 3, msg.Length);

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

        public void SetClientGuid(AsyncClient client)
        {
            ServerClientProtocol p = new ServerClientProtocol("01");
            p.clientGuid = client.clientGuid;
            byte[] cmd = p.Serialize(p);
            Write(client.client, cmd);
        }

        public void RemoveClient(Guid g)
        {
            //try to find the ship, 
            //AsyncClient clientFound = clients.Find(client => client.clientGuid == g);
            
            
            if (this.clients.Find(client => client.clientGuid == g) != null)
            {
                bot c = this.clients.Find(cl => cl.clientGuid == g);
                lock (this.clients)
                {
                    clients.Remove(c);
                }
                AddToLog("Client: " + c.clientGuid.ToString() + " disconnected...");
            }
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
                byte[] lengthBufferBytes = new byte[2];
                Buffer.BlockCopy(msg, 1, lengthBufferBytes, 0, 2);
                //als 2 waardes geen char- getal zijn, return 0;
                string aS = lengthBufferBytes[0].ToString();
                string bS = lengthBufferBytes[1].ToString();
                char a = aS[0];
                char b = bS[0];
                if (a >= '0' && a <= '9')
                {
                    if (b >= '0' && b <= '1')
                    {
                        int lengthBuffer = BitConverter.ToInt16(lengthBufferBytes, 0);
                        return lengthBuffer;
                    }
                }
                return 0;

            }
            catch (Exception ex)
            {
                return 0;
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
                    byte[] bot;
                    MemoryStream ms = new MemoryStream();
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(ms, o);
                    bot = ms.ToArray();
                    ms.Close();
                    return bot;
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

                bot client = new bot(tcpClient, buffer);
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
                AddToLog("A bot just connected, given the id : " + client.clientGuid.ToString());
                //RequestClientGuid(client);
                //SetClientGuid(client);
                //begin accepting new clients:
                tcpListener.BeginAcceptTcpClient(AcceptTcpClientCallback, null);
                setBotList();
                UpdateForm();
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
                    bot client = asyncResult.AsyncState as bot;
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
                        lock (this.clients)
                        {
                            this.clients.Remove(client);
                        }
                        setBotList();
                    } 

                    if (buffer != null)
                    {
                        Buffer.BlockCopy(buffer, 0, data, 0, read);
                        if (this.DataRead != null)
                            this.DataRead(this, new DataReadEventArgs(data));
                    }

                    
                    nrBytesToRead = GetMsgSize(data) + 3;
                    
                    fullData = new byte[nrBytesToRead];
                    
                    if (nrBytesToRead <= read)
                    {
                        Buffer.BlockCopy(data, 0, fullData, 0, nrBytesToRead);
                    }
                    else
                    {
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
                            else
                            {
                                _read = ns.Read(buffer, 0, DefaultClientReadBufferLength);
                                Buffer.BlockCopy(buffer, 0, fullData, bytesReadSoFar, _read);
                                bytesReadSoFar += _read;
                            }
                        }
                    }

                    //deframe msg
                    //fullData = DeFrameMsg(fullData);

                    //update the log
                    //AddToLog("received: " + fullData.GetLength(0).ToString() + " bytes from client: " + client.clientGuid.ToString());


                    if (this.DataRead != null)
                        this.DataRead(this, new DataReadEventArgs(data));
                    string dataString = GetString((data));
                    string msg = dataString.Substring(3);
                    ParseBotCmd(data, client.clientGuid);
                    AddToLog(client.clientGuid.ToString() + " wrote: " + msg);
                    Guid botGuid = client.clientGuid;
                    

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
                    setBotList();
                    //AddToLog("client with handle: " + client.clientGuid.ToString() + " disconnected...");
                    UpdateForm();

                }
            }
        }
        #endregion
    }
}
