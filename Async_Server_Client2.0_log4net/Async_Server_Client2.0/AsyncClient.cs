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
    [Serializable]
    public class AsyncClient
    {
        #region Variables
        /// <summary>
        /// default length for client read buffer
        /// </summary>
        private const int DefaultClientReadBufferLength = 4096;


        /// <summary>
        /// the port to connect to
        /// </summary>
        private readonly int port;

        /// <summary>
        /// length of readbuffer
        /// </summary>
        private readonly int clientReadBufferLength;

        /// <summary>
        /// the adresses to connect to
        /// </summary>
        private IPAddress[] addresses;

        /// <summary>
        /// nr of connection retries
        /// </summary>
        private int retries;

        /// <summary>
        /// client buffer
        /// </summary>
        public byte[] clientBuffer { get; set; }

        public List<Server> FrontEndList { get; set; }

        /// <summary>
        /// bool client connected?
        /// </summary>
        public bool isConnected { get; set; }

        public bool guidConfirmed { get; set; }

        public Guid clientGuid { get; set; }
        public Guid ControlledBy { get; set; }
        public Guid isControlling { get; set; }

        public delegate void RefreshForm();
        public event RefreshForm UpdateForm;

        public delegate void ParseControllerCommand(byte[] cmd);
        public event ParseControllerCommand ParseRgbCmd;

        public delegate void updateList(List<Guid> clientList);
        public updateList setClientList;

        public delegate void updateBotList(List<Guid> botList);
        public updateBotList setBotList;

        public delegate void ServerDisconnected();
        public ServerDisconnected ServerGone;

        public delegate void chatMsg(string msg);
        public chatMsg setChatMsg;

        public delegate void setClientBot(bot bot);
        public setClientBot setBot;

        [NonSerialized]

        /// <summary>
        /// the tcp client for outgoing connections
        /// </summary>
        public TcpClient client;


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
        /// constructor based on a number of IP adresses
        /// </summary>
        /// <param name="addresses"></param>
        /// <param name="port"></param>
        /// <param name="clientReadBufferLength"></param>
        public AsyncClient(IPAddress[] addresses, int port, int clientReadBufferLength = DefaultClientReadBufferLength)
            : this(port, clientReadBufferLength)
        {
            this.addresses = addresses;
        }


        /// <summary>
        /// constructor based on an IP address and port.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <param name="clientReadBufferLength"></param>
        public AsyncClient(IPAddress address, int port, int clientReadBufferLength = DefaultClientReadBufferLength)
            : this(new[] { address }, port, clientReadBufferLength)
        {

        }

        /// <summary>
        /// constructor based on a TCP client
        /// </summary>
        /// <param name="tcpClient"></param>
        /// <param name="buffer"></param>
        public AsyncClient(TcpClient tcpClient, byte[] buffer)
        {
            this.client = tcpClient;
            this.clientBuffer = buffer;
        }

        public AsyncClient()
        { }

        /// <summary>
        /// private constructor for a new AsyncClient object
        /// </summary>
        /// <param name="port"></param>
        /// <param name="clientReadBufferLength"></param>
        private AsyncClient(int port, int clientReadBufferLength)
        {
            this.client = new TcpClient();
            this.port = port;
            this.clientReadBufferLength = clientReadBufferLength;
            this.isConnected = false;
        }


        #endregion

        #region Methods

        /// <summary>
        /// starts a connection to the server:
        /// </summary>
        public void Connect()
        {
            try
            {
                this.client.BeginConnect(this.addresses, this.port, this.ClientConnectCallback, null);

            }
            catch (Exception e)
            {

            }
        }

        public void Disconnect()
        {
            this.client.GetStream().Close();
            this.client.Close();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public Guid Write(string value, Encoding encoding)
        {
            byte[] buffer = encoding.GetBytes(value);
            return this.Write(buffer);
        }

        /// <summary>
        /// write a byte array to the server
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public Guid Write(byte[] buffer)
        {
            Guid guid = Guid.NewGuid();
            NetworkStream ns = this.client.GetStream();
            byte[] data = FrameMsg(buffer);
            ns.BeginWrite(data, 0, data.Length, this.ClientWriteCallback, guid);
            return guid;
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
                //copy the length of the msg to the first 4 bytes to the data[]
                Buffer.BlockCopy(msgLengthBytes, 0, data, 0, 4);

                //copy the rest to the data[]
                Buffer.BlockCopy(msg, 0, data, 4, msg.Length);

                return data;
            }
            catch (Exception e)
            {
                return null;
            }
        }


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
        public void RequestFrontEndList()
        {
            ServerClientProtocol p = new ServerClientProtocol("04");
            p.clientGuid = this.clientGuid;
            byte[] cmd = p.Serialize(p);
            Write(cmd);
        }

        public void RequestRgbControllerList()
        {
            ServerClientProtocol p = new ServerClientProtocol("06");
            p.clientGuid = this.clientGuid;
            byte[] cmd = p.Serialize(p);
            Write(cmd);
        }
        public void RequestStarShipList()
        {
            ServerClientProtocol p = new ServerClientProtocol(this.clientGuid, "11");
            byte[] cmd = p.Serialize(p);
            Write(cmd);
        }



        private void SetGuid(Guid g)
        {
            this.clientGuid = g;
            ServerClientProtocol p = new ServerClientProtocol(g, "01");
            WriteP(p);
        }

        public void WriteP(ServerClientProtocol p)
        {
            byte[] cmd = p.Serialize(p);
            //cmd = FrameMsg(cmd);
            Write(cmd);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        public void ParseServerClientProtocol(ServerClientProtocol p)
        {
            try
            {
                switch (p.command)
                {
                    case "00":
                        //server requests Guid:
                        ServerClientProtocol resp = new ServerClientProtocol("02");
                        resp.clientGuid = this.clientGuid;
                        byte[] respArray = p.Serialize(resp);
                        Write(respArray);

                        break;
                    case "01":
                        //Server assigned a Guid to us, set it!
                        SetGuid(p.clientGuid);
                        UpdateForm();
                        break;
                    case "02":
                        //client received a list of clients
                        List<Guid> feList = new List<Guid>();
                        List<Guid> botList = new List<Guid>();
                        feList = p.ClientList;
                        botList = p.BotList;
                        setClientList(feList);
                        setBotList(botList);
                        UpdateForm();
                        break;
                    case "03":
                        //We received a chat message!
                        string msg = p.Parameters[0];
                        setChatMsg(msg);

                        break;
                    case "04":
                        //we locked a bot!
                        this.isControlling = p.botGuid;
                        UpdateForm();
                        break;
                    case "05":
                        //Server sent us a list.
                        int typeOfList = p.typeOfList;
                        if (typeOfList == 0)
                        {
                            //this.FrontEndList = p.clientList;
                        }
                        else if (typeOfList == 1)
                        {
                            //this.RGBcontrollerList = p.clientList;
                        }
                        else if (typeOfList == 2)
                        {
                            //List<Starship> list = p.shipList;
                            //SetStarShipList(list);
                        }
                        UpdateForm();
                        break;
                    case "08":

                        break;
                    case "09":
                        //we locked a bot!
                        break;
                    case "10":
                        //bot was allready locked..
                        break;
                    case "11":
                        //bot has send a response to server!
                        setBot(p.bot);
                        //parse RSP..
                        break;
                }
            }
            catch (Exception ex)
            {

            }
        }


        public static object Deserialize(byte[] _object)
        {
            int read = _object.Length;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream(_object))
            {
                try
                {
                    object o = bf.Deserialize(ms);
                    if (o.GetType() == typeof(ServerClientProtocol))
                    {
                        ServerClientProtocol p = (ServerClientProtocol)o;
                        return p;
                    }
                    else if (o.GetType() == typeof(bot))
                    {
                        bot bot = (bot)o;
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
        }

        #endregion

        #region callbacks

        /// <summary>
        /// Callback from the async Connect method
        /// </summary>
        /// <param name="asyncResult"></param>
        private void ClientConnectCallback(IAsyncResult asyncResult)
        {
            try
            {
                this.client.EndConnect(asyncResult);
                if (this.Connected != null)
                {
                    this.Connected(this, new EventArgs());
                    this.isConnected = true;
                    UpdateForm();

                }
            }
            catch (Exception e)
            {
                retries++;
                if (retries < 3)
                {
                    this.client.BeginConnect(this.addresses, this.port, this.ClientConnectCallback, null);
                }
                else
                {
                    if (this.ClientConnectException != null)
                        this.ClientConnectException(this, new ExceptionEventArgs(e));
                }

            }

            try
            {
                NetworkStream ns = this.client.GetStream();
                byte[] buffer = new byte[this.clientReadBufferLength];
                ns.BeginRead(buffer, 0, buffer.Length, this.ClientReadCallback, buffer);
                this.isConnected = true;
            }
            catch (Exception e)
            {
                if (this.ClientReadException != null)
                    this.ClientReadException(this, new ExceptionEventArgs(e));
            }

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="asyncResult"></param>
        private void ClientReadCallback(IAsyncResult asyncResult)
        {
            try
            {
                NetworkStream ns = this.client.GetStream();
                int read = ns.EndRead(asyncResult);
                byte[] data = new byte[read];
                int nrBytesToRead = 0;
                int bytesReadSoFar = 0;
                byte[] buffer = asyncResult.AsyncState as byte[];
                byte[] fullData;


                //if read == 0 dan heeft server verbinding verbroken..
                if (read == 0)
                {
                    ServerGone();
                    if (this.Disconnected != null)
                        this.Disconnected(this, new EventArgs());
                }


                if (buffer != null)
                {
                    Buffer.BlockCopy(buffer, 0, data, 0, read);
                    if (this.DataRead != null)
                        this.DataRead(this, new DataReadEventArgs(data));

                    //ns.BeginRead(buffer, 0, buffer.Length, this.ClientReadCallback, buffer);
                }

                nrBytesToRead = GetMsgSize(data) + 4;
                fullData = new byte[nrBytesToRead];
                if (nrBytesToRead <= read)
                {
                    Buffer.BlockCopy(data, 0, fullData, 0, nrBytesToRead);
                    bytesReadSoFar += nrBytesToRead;
                }
                else
                {
                    Buffer.BlockCopy(data, 0, fullData, 0, read);
                    bytesReadSoFar += read;

                    while (nrBytesToRead > bytesReadSoFar)
                    {
                        //laatste restje..
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
                        //ns.BeginRead(buffer, 0, buffer.Length, this.ClientReadCallback, buffer);
                    }
                }




                fullData = DeFrameMsg(fullData);



                if (this.DataRead != null)
                    this.DataRead(this, new DataReadEventArgs(data));

                //now we got the data deserialize it:
                object tmpObject = Deserialize(fullData);
                if (tmpObject.GetType() == typeof(ServerClientProtocol))
                {
                    ServerClientProtocol p = (ServerClientProtocol)tmpObject;
                    ParseServerClientProtocol(p);
                }
                else if (tmpObject.GetType() == typeof(bot))
                {
                    bot b = (bot)tmpObject;
                    setBot(b);
                }
                ns.BeginRead(buffer, 0, buffer.Length, this.ClientReadCallback, buffer);
                UpdateForm();
            }
            catch (IOException e)
            {
                ServerGone();
            }
            catch (Exception e)
            { 
                if (this.ClientReadException != null)
                    this.ClientReadException(this, new ExceptionEventArgs(e));
            }


        }

        private void ClientWriteCallback(IAsyncResult asyncResult)
        {
            try
            {
                NetworkStream ns = this.client.GetStream();
                ns.EndWrite(asyncResult);
                Guid guid = (Guid)asyncResult.AsyncState;
                if (this.DataWritten != null)
                    this.DataWritten(this, new DataWrittenEventArgs(guid));
            }
            catch (Exception e)
            {
                if (this.ClientWriteException != null)
                    this.ClientWriteException(this, new ExceptionEventArgs(e));
            }
        }

        #endregion
    } //end of class

    [Serializable]
    public class bot : AsyncClient
    {
        #region variables
        public int sensorVal { get; set; }
        public bool led { get; set; }
        #endregion

        #region constructors
        public bot()
            : base()
        {
            this.led = true;
            this.sensorVal = 0;
        }

        public bot(TcpClient client, byte[] buffer)
            : base(client, buffer)
        {
            this.led = true;
            this.sensorVal = 0;
        }
        #endregion
    }

    #region Exception class
    public class ExceptionEventArgs : EventArgs
    {
        public Exception Exception { get; private set; }
        public ExceptionEventArgs(Exception e)
        {
            this.Exception = e;
        }
    }

    public class DataReadEventArgs : EventArgs
    {
        /// <summary>
        /// constructor for a new Data Read Event Args object
        /// </summary>
        /// <param name="data"></param>
        public DataReadEventArgs(byte[] data)
        {
            this.Data = data;
        }
        /// <summary>
        /// gets the data that has been read
        /// </summary>
        public byte[] Data { get; private set; }
    }

    public class DataWrittenEventArgs : EventArgs
    {
        /// <summary>
        /// constructor for a Data written Event Args object
        /// </summary>
        /// <param name="guid"></param>
        public DataWrittenEventArgs(Guid guid)
        {
            this.Guid = guid;
        }
        /// <summary>
        /// get the guid used to match the data written to the confirmation event
        /// </summary>
        public Guid Guid { get; private set; }
    }
    #endregion

}
