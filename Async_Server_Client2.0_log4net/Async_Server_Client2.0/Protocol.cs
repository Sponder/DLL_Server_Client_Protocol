using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;


namespace Async_Server_Client2._0
{
    [Serializable]
    public class ServerClientProtocol
    {
        public string command { get; set; }
        public Guid clientGuid { get; set; }
        public Guid botGuid { get; set; }
        public Guid sendToClient { get; set; }
        public Guid sendToBot { get; set; }
        private List<string> parameters = new List<string>();
        public List<string> Parameters { get { return parameters; } }
        private List<Guid> clientList = new List<Guid>();
        public List<Guid> ClientList { get { return clientList; } }
        private List<Guid> botList = new List<Guid>();
        public List<Guid> BotList { get { return botList; } }
        public string botAnswer { get; set; }
        public bot bot { get; set; }

        /// <summary>
        /// int type of list
        /// 0 FrontEndList
        /// 1 RgbList
        /// </summary>
        public int typeOfList { get; set; }

        /*
         * Command List:
         *
         * "00" ServerCommand request the Guid from the client
         * "01" ServerCommand to let the client set the Guid the server gave the client(Guid clientGuid)
         * "02" ClientCommand to let the server know it set its Guid(Guid clientGuid) 
         * "03" ClientCommand Request a new FrontEndList
         * "04" ClientCommand to ACK the frontEndclientList<AsyncClient>
         * "05" ServerComand to Update the clients list of rgbControllerList<AsyncClient>
         * "06" ClientCommand to ACK the rgbControllerList<AsyncClient>
         * "10" ClientCommand to let the server send a command to a RGBcontroller(string command, AsyncClient RgbBoard)
         *      //idee tbv redundancy: RgbBoard.clientGuid opzoeken in server.RgbLedServer.clients en daar naar toe sturen?
         */

        public ServerClientProtocol()
        {

        }


        public ServerClientProtocol(string command)
        {
            this.command = command;
        }

        public ServerClientProtocol(Guid clientGuid, string command)
            : this(command)
        {
            this.clientGuid = clientGuid;
        }

        public ServerClientProtocol(bot bot)
        : this()
        {
            this.bot = bot;
        }

        public void AddParameter(string parameter)
        {
            Parameters.Add(parameter);
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

        /// <summary>
        /// Deserialize an object
        /// </summary>
        /// <param name="_object"></param>
        /// <returns>The object which was serialized</returns>
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
                    else if (o.GetType() == typeof(List<AsyncClient>))
                    {
                        List<AsyncClient> list = (List<AsyncClient>)o;
                        return list;
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

    }
}
