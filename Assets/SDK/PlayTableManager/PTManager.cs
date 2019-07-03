using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using UnityEngine.SceneManagement;

namespace PlayTable.Unity
{
    /*The manager that developer use directly*/
    public sealed class PTManager : MonoBehaviour
    {
        /*public fields*/
        //Debug Mode
        [HideInInspector]
        public bool debugMode = false;
        //Dont destroy
        [HideInInspector]
        public bool dontDestroyOnLoad = true;
        //Game name showing to players
        public static string gameName = "";
        public static string playerName = "";

        /*Tabletop Side*/
        //Accept new players to join
        [HideInInspector]
        public bool acceptNewPlayer = true;
        //Minimum players required for the game
        public static int minimumPlayer = DEFAULT_MINIMUN_PLAYER;
        //Maximun players limited by the spawn slots
        public static int maximumPlayer = DEFAULT_MAXMUN_PLAYER;

        /*Readonly*/
        //The only instance of the class
        public static PTManager singleton = null;
        //Discovered sessions in local network
        public static HashSet<PTSession> NearbySessions { get { return nearbySessions; } }
        //Connected players actually playing the game. NOT the ones in the wait queue
        public static Dictionary<PTPlayer, bool>.KeyCollection Players { get { return players.Keys; } }
        //Connection status
        public static bool isConnected {
            get
            {
                return(singleton.myClient!=null
                   && (singleton.myClient.isConnected || NetworkServer.connections.Count > 0));
            }
        }
        //Manager Info
        public static string Info
        {
            get
            {
                return singleton.ToString();
            }
        }
        //Get device type. (isTableTop==false) == isHandheld
        public static bool isTableTop
        {
            get
            {
                return NetworkServer.active;
            }
        }
        //int timecreated
        public readonly DateTime timeCreated_UTC = DateTime.UtcNow;
        //is currenly working
        public static bool isRunning { get {
                return NetworkServer.active || 
                    (singleton != null && singleton.myClient != null && singleton.myClient.isConnected);
            } }

        /*internal fields*/
        //local client instance
        NetworkClient myClient;
        //session finder and receiver
        NetworkDiscovery nd;

        //connected players <PTPlayer, ready>
        static Dictionary<PTPlayer, bool> players = new Dictionary<PTPlayer, bool>();

        //discovered nearby sessions <sessionName, session>
        static HashSet<PTSession> nearbySessions = new HashSet<PTSession>();

        //waiting queque for connected players <connectedName, connectionId>
        static Queue<KeyValuePair<string, int>> waitQueue = new Queue<KeyValuePair<string, int>>();

        //Default min player num
        const int DEFAULT_MINIMUN_PLAYER = 1;
        //Default max player num
        const int DEFAULT_MAXMUN_PLAYER = 4;

        //Session life
        const float SESSION_LIFE = 3;
        //Temp session set for garbage clean
        HashSet<PTSession> tempNearbySessionSet = new HashSet<PTSession>();

        /*API*/
        public static void Connect(string sender, string game)
        {
            PTSession session = PTSession.Find(sender, game);
            singleton.StartCoroutine(singleton.ConnectCoroutine(session));
        }
        public static void Send(PTPlayer player, byte[] data)
        {
            //This method is the parent method for other server side overloads

            //Corner case
            if (singleton == null || !NetworkServer.active || player == null)
            {
                Debug.Log("Error in PTManager.Send(PTPlayer player, byte[] data): Check singleton, server settings or PTPlayer instance");
                return;
            }

            //create pt message to send
            PTMessage msg = new PTMessage();
            msg.senderName = playerName;
            msg.data = data;

            //Send the pt message
            NetworkServer.SendToClient(player.connectionId, (short)PTEvent.Data, msg);
            singleton.PTDebug("Sent to " + player + ": " + msg.ToString(true));

        }
        public static void Send<T>(PTPlayer player, T data)
        {
            if (typeof(T) == typeof(byte[]))
            {
                Send(player, data as byte[]);
            }
            else
            {
                byte[] tosendData = Encoding.Unicode.GetBytes(data.ToString());
                Send(player, tosendData);
            }
        }
        public static void Send<T>(string playerName, T data)
        {
            PTPlayer player = PTPlayer.Find(playerName);
            Send<T>(player, data);
        }
        public static void Send(byte[] data)
        {
            PTMessage msg = new PTMessage();
            msg.senderName = playerName;
            msg.data = data;

            if (NetworkServer.active)
            {
                //Send to all clients if the sender is a TableTop
                foreach (PTPlayer player in Players)
                {
                    Send(player, data);
                }
            }
            else if (singleton.myClient != null && singleton.myClient.isConnected)
            {
                //Send to connected TableTop if the sender is a Handheld
                if (singleton.myClient.connection != null)
                {
                    singleton.myClient.Send((short)PTEvent.Data, msg);
                    singleton.PTDebug("Sent to server: " + msg.ToString(true));

                }
                else
                {
                    singleton.PTDebug("PTManager Error: built-in client is Null");
                }
            }
        }
        public static void Send(string data)
        {
            Send(Encoding.Unicode.GetBytes(data));
        }
        public static void Send<T>(T data)
        {
            string json = JsonUtility.ToJson(data);
            Send(json);
        }
        public static void ReportReady()
        {
            PTMessage msg = new PTMessage();
            msg.senderName = playerName;
            msg.gameName = gameName;
            if (isTableTop)
            {
                foreach (PTPlayer player in Players)
                {
                    NetworkServer.SendToClient(player.connectionId, (short)PTEvent.Ready, msg);
                }
            }
            else if (singleton.myClient != null && singleton.myClient.isConnected)
            {
                singleton.myClient.Send((short)PTEvent.Ready, msg);
            }
            else
            {
                Debug.Log("Report Ready Error");
            }
        }
        [Obsolete("[deprecated] Will be deleted any time. Use Initialize instead.")]
        public static void StartNetwork(bool isTabletop)
        {
            //direct to initialize
            Initialize(isTabletop);
            
            //count method called by developers, to get the usage frequency

        }
        public static void Initialize(bool isTabletop)
        {
            //Create a game object for instance if there was no one
            if (singleton == null)
            {
                GameObject ptManager = new GameObject();
                singleton = ptManager.AddComponent<PTManager>();
            }

            //Start nearby session finder
            singleton.ResetNetworkoDiscovery();

            //Start server or client
            if (isTabletop)
            {
                singleton.StartServer();
            }
            else
            {
                singleton.StartClient();
            }

            //Clear players fields
            players.Clear();
            nearbySessions.Clear();
        }
        public static void Initialize(bool isTabletop, bool onDebugMode)
        {
            //Base
            Initialize(isTabletop);

            //set debug mode
            singleton.debugMode = onDebugMode;
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("**PlayTable Manager Info**\n")
                .Append(" playerName=" + playerName)
                .Append(" gameName=" + gameName)
                .Append(" acceptNewPlayer=" + acceptNewPlayer)
                .Append(" minimumPlayer=" + minimumPlayer)
                .Append(" maximumPlayer=" + maximumPlayer)
                ;

            if (singleton.myClient != null)
            {
                sb.Append("[Client Info]{")
                    .Append("ConnectionId=" + singleton.myClient.connection.connectionId)
                    .Append(" hostId=" + singleton.myClient.connection.hostId)
                    .Append(" handlers.Count=" + singleton.myClient.handlers.Count)
                    .Append(" DefaultConfig=" + singleton.myClient.hostTopology.DefaultConfig)
                    .Append("}\n");
            }
            return sb.ToString();
        }
        public static bool isReady(PTPlayer player)
        {
            try
            {
                return players[player];
            }
            catch
            {
                return false;
            }
        }

        /*Public delegates*/
        public delegate void PTBroadcastDelegate(PTSession session);
        public static PTBroadcastDelegate OnBroadcastReceieved;
        public delegate void PTMessageDelegate(PTMessage msg);
        public static PTMessageDelegate OnConnected;
        public static PTMessageDelegate OnDisconnected;
        public static PTMessageDelegate OnDataReceived;
        public static PTMessageDelegate OnReadyReceived;
        public static PTMessageDelegate OnConfirmReadyReceived;
        public delegate void PTSmartPieceDelegate(SmartPiece sp);
        public static PTSmartPieceDelegate OnSmartPiece;

        /*Internal methods*/
        void SingletonCheck()
        {
            //singleton feature
            if (singleton == null)
            {
                singleton = this;
            }
            else if (singleton != this)
            {
                Destroy(gameObject);
            }
        }
        IEnumerator NamesCheck()
        {
            float timer = 1;
            while (true)
            {
                //name = playerName;
                if (gameName == default(string) || gameName == "")
                    gameName = SceneManager.GetActiveScene().name;
                yield return new WaitForSeconds(timer);
            }
        }
        void ResetNetworkoDiscovery()
        {
            /*init network discovery*/
            //clear all existing NetworkDiscovery

            nd = GetComponent<PTDiscovery>();
            if (!nd)
            {
                if (GetComponent<NetworkDiscovery>())
                {
                    Destroy(GetComponent<NetworkDiscovery>());
                }
                nd = gameObject.AddComponent<PTDiscovery>();
            }

            //Misc settings
            nd.showGUI = false;
        }
        void StartServer()
        {
            if (!NetworkServer.active)
            {
                NetworkServer.RegisterHandler((short)PTEvent.Connect, Handler_OnConnected);
                NetworkServer.RegisterHandler(MsgType.Disconnect, Handler_OnDisconnect);
                NetworkServer.RegisterHandler((short)PTEvent.Data, Handler_OnData);
                NetworkServer.RegisterHandler((short)PTEvent.Ready, Handler_OnReadyReceived);
                NetworkServer.RegisterHandler((short)PTEvent.ConfirmReadyReceived, Handler_OnConfirmReady);
                NetworkServer.Listen(PTNetwork.DEFAULT_SERVER_PORT);

            }

            PTSession newSession = new PTSession();
            newSession.senderName = playerName;
            newSession.gameName = gameName;
            newSession.port = NetworkServer.listenPort;
            newSession.data = Encoding.Unicode.GetBytes(PTNetwork.DEFAULT_BROADCAST_DATA);

            if (nd.running)
            {
                nd.StopBroadcast();
            }
            nd.broadcastData = newSession.ToString();
            nd.Initialize();
            nd.StartAsServer();
        }
        void StartClient()
        {
            StartCoroutine(StartClientCoroutine());
        }
        IEnumerator StartClientCoroutine()
        {
            ResetClient();

            if (nd.running)
            {
                nd.StopBroadcast();
            }
            yield return new WaitUntil(() => !nd.running);
            nd.Initialize();
            nd.StartAsClient();
        }
        void ResetClient()
        {
            if (myClient != null)
            {
                return;
            }
            myClient = new NetworkClient();

            myClient.RegisterHandler((short)PTEvent.Connect, Handler_OnConnected);
            myClient.RegisterHandler(MsgType.Disconnect, Handler_OnDisconnect);
            myClient.RegisterHandler((short)PTEvent.Data, Handler_OnData);
            myClient.RegisterHandler((short)PTEvent.Ready, Handler_OnReadyReceived);
            myClient.RegisterHandler((short)PTEvent.ConfirmReadyReceived, Handler_OnConfirmReady);
        }
        void ConfirmReadyReceived(NetworkMessage received)
        {
            PTMessage msg = new PTMessage();
            msg.senderName = playerName;
            msg.gameName = gameName;
            msg.data = Encoding.Unicode.GetBytes("ready recieved");
            if (isTableTop)
            {
                NetworkServer.SendToClient(received.conn.connectionId, (short)PTEvent.ConfirmReadyReceived, msg);
            }
            else if (myClient != null)
            {
                myClient.Send((short)PTEvent.ConfirmReadyReceived, msg);
            }
        }
        IEnumerator<CustomYieldInstruction> ConnectCoroutine(PTSession session)
        {
            if (!myClient.isConnected && session != null)
            {
                //
                ResetClient();

                //Check the player's name is valid (no duplica)
                //bool isClientNameValid = true;
                if (isClientNameValid())
                {
                    //Actuall connect happens here
                    myClient.Connect(session.ip, session.port);

                    //wait until the connection is established
                    yield return new WaitUntil(() => myClient.isConnected);

                    //Send the connected event msg to server
                    PTMessage msg = new PTMessage();
                    msg.senderName = playerName;
                    myClient.Send((short)PTEvent.Connect, msg);
                }
            }
        }
        private bool isClientNameValid()
        {
            //Not implemented. Always return true.
            return true;
        }
        IEnumerator NearbySessionsGarbageClean()
        {
            /*tempNearbySessionSet is cleared every SESSION_LIFE seconds
             * During the SESSION_LIFE seconds, tempNearbySessionSet is 
             * filled with newly coming sessions. The nearbySession set 
             * is updated every SESSION_LIFE seconds according to the 
             * tempNearbySessionSet
             **/
            while (true)
            {
                tempNearbySessionSet.Clear();
                yield return new WaitForSeconds(SESSION_LIFE);
                nearbySessions.Clear();
                foreach (PTSession session in tempNearbySessionSet)
                {
                    nearbySessions.Add(session);
                }
            }
        }
        void Handler_OnDisconnect(NetworkMessage netMsg)
        {
            PTPlayer player = PTPlayer.Find(netMsg.conn.connectionId);
            if (player != null)
            {
                players[player] = false;
            }
            string logText = player == null ? "OnDisconnected" : "OnDisconnected: " + player;
            PTDebug(logText);

            if (isTableTop)
            {
                try {
                    players.Remove(player);
                } catch { }
            }
            else
            {
                //Destroy(gameObject);
                /*
                Debug.Log("hostId=" + myClient.connection.hostId);

                myClient.connection.UnregisterHandler((short)PTEvent.Connect);
                myClient.connection.UnregisterHandler(MsgType.Disconnect);
                myClient.connection.UnregisterHandler((short)PTEvent.Data);
                myClient.connection.UnregisterHandler((short)PTEvent.Ready);
                myClient.connection.UnregisterHandler((short)PTEvent.ConfirmReadyReceived);

                myClient.connection.Disconnect();
                myClient.connection.Dispose();
                myClient.connection.ResetStats();
                myClient.connection.connectionId = -1;

                myClient.UnregisterHandler((short)PTEvent.Connect);
                myClient.UnregisterHandler(MsgType.Disconnect);
                myClient.UnregisterHandler((short)PTEvent.Data);
                myClient.UnregisterHandler((short)PTEvent.Ready);
                myClient.UnregisterHandler((short)PTEvent.ConfirmReadyReceived);
                myClient.ResetConnectionStats();
                myClient.Disconnect();
                myClient.Shutdown();

                NetworkClient.ShutdownAll();
                //NetworkTransport.RemoveHost(myClient.connection.hostId);
                */


                players.Clear();
            }
            if (OnDisconnected != null)
            {
                PTMessage ptMsg = new PTMessage();
                ptMsg.senderName = isTableTop ? player.name : "PlayTable";
                ptMsg.gameName = gameName;
                OnDisconnected(ptMsg);
            }
        }
        void Handler_OnData(NetworkMessage netMsg)
        {
            PTMessage msg = netMsg.ReadMessage<PTMessage>();
            PTDebug("OnDataReceived: " + msg.ToString(true));

            if (OnDataReceived != null)
            {
                OnDataReceived(msg);
            }
        }
        void Handler_OnConnected(NetworkMessage netMsg)
        {

            PTMessage msg = netMsg.ReadMessage<PTMessage>();
            PTDebug("OnConnected: " + msg.ToString(true));

            PTPlayer player = PTPlayer.Find(netMsg.conn.connectionId);
            if (player != null)
            {
                players[player] = false;
            }
            else
            {
                PTPlayer newPlayer = new PTPlayer(msg.senderName, netMsg.conn.connectionId, null);
                players.Add(newPlayer, false);
            }

            if (isTableTop)
            {
                if (!acceptNewPlayer || players.Count >= maximumPlayer)
                {
                    //put the newly connected player in the wait queue
                    KeyValuePair<string, int> newPlayer = new KeyValuePair<string, int>(msg.senderName, netMsg.conn.connectionId);
                    waitQueue.Enqueue(newPlayer);
                }

                //Tell the sender client that this server has been connected
                PTMessage msgFromServer = new PTMessage();
                msgFromServer.senderName = playerName; NetworkServer.SendToClient(netMsg.conn.connectionId, (short)PTEvent.Connect, msgFromServer);
            }
            else
            {
            }

            //Invoke OnConnected Event
            if (OnConnected != null)
            {
                OnConnected(msg);
            }
        }
        void Handler_OnBroadcastReceieved(PTSession session)
        {
            PTDebug("OnBroadcastReceieved: " + session.ToString(true));

            try
            {
                nearbySessions.Add(session);
                tempNearbySessionSet.Add(session);
            }
            catch
            {

            }
        }
        void Handler_OnReadyReceived(NetworkMessage netMsg)
        {
            PTMessage msg = netMsg.ReadMessage<PTMessage>();
            PTDebug("OnReady: " + msg.ToString(true));
            PTPlayer player = PTPlayer.Find(msg.senderName);
            if (player != null)
            {
                players[player] = true;
            }
            if (OnReadyReceived != null)
            {
                OnReadyReceived(msg);
            }
            ConfirmReadyReceived(netMsg);
        }
        void Handler_OnConfirmReady(NetworkMessage netMsg)
        {
            PTMessage msg = netMsg.ReadMessage<PTMessage>();
            PTDebug("OnConfirmReady: " + msg.ToString(true));

            if (OnConfirmReadyReceived != null)
            {
                OnConfirmReadyReceived(msg);
            }
        }
        void PTDebug(string content)
        {
            if (debugMode)
            {
                if (Application.platform == RuntimePlatform.WindowsEditor
                    || Application.platform == RuntimePlatform.OSXEditor
                    || Application.platform == RuntimePlatform.LinuxEditor)
                {
                    Debug.Log(content);
                }
                else if (Application.platform == RuntimePlatform.WindowsPlayer
                    || Application.platform == RuntimePlatform.Android
                    || Application.platform == RuntimePlatform.IPhonePlayer
                    || Application.platform == RuntimePlatform.OSXPlayer)
                {
                    Debug.LogError(content);
                }
            }

        }
        IEnumerator ListenSmartPiece()
        {
            while (true)
            {
                if (OnSmartPiece != null)
                {
                    SmartPiece sp = SmartPiece.Get();
                    if (sp != null)
                    {
                        OnSmartPiece(sp);
                    }
                }

                yield return new WaitForEndOfFrame();
            }
        }
        void Handler_OnSmartPiece(SmartPiece sp)
        {
            PTDebug("OnSmartPiece" + sp.ToString());
        }

        /*Unity Built-in mono methods*/
        private void Awake()
        {
            //If the player's name is default value, then change it with first ten digits of unique device identifier
            playerName = playerName == "" ? SystemInfo.deviceUniqueIdentifier.Substring(0, 10) : playerName;

            //dont destroy this
            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(this);
            }

            //
            OnBroadcastReceieved += Handler_OnBroadcastReceieved;

            //
            players.Clear();

            //SmartPiece
            OnSmartPiece += Handler_OnSmartPiece;

            //
            StartCoroutine(NamesCheck());
            SingletonCheck();
            StartCoroutine(NearbySessionsGarbageClean());
            StartCoroutine(ListenSmartPiece());
        }
    }

    /*Manager class for PlayTable network*/
    public class PTNetwork
    {
        public const int PORT_MIN = 1, PORT_MAX = 65535;
        public const int DEFAULT_MAX_PALYER = 4;
        public const short DEFAULT_SERVER_PORT = 1015;
        public const short DEFAULT_CLIENT_PORT = 527;
        public const bool DEFAULT_USE_LOCAL_DISCOVERY = true;
        public const string DEFAULT_BROADCAST_DATA = "Hello from PlayTable";
        public const int DEFAULT_BUFFER_SIZE = 512;
    }

    /*Types of event sending between TT and HH*/
    public enum PTEvent
    {
        Data = MsgType.Highest + 1,
        Connect,
        Disconnect,
        Ready,
        ConfirmReadyReceived
    }

    /*Message used to communicate between Tabletop and Handheld*/
    public class PTMessage : MessageBase
    {
        public string senderName;
        public string gameName;
        public byte[] data;

        public static PTMessage FromJson(string json)
        {
            return JsonUtility.FromJson<PTMessage>(json);
        }
        public override string ToString()
        {
            return JsonUtility.ToJson(this);
        }
        public string ToString(bool enCoding)
        {
            if (enCoding)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("data=").Append(Encoding.Unicode.GetString(data))
                    .Append(" senderName=").Append(senderName);
                return sb.ToString();
            }
            else
            {
                return ToString();
            }
        }
        public string GetData()
        {
            return Encoding.Unicode.GetString(data);
        }
        public override bool Equals(object obj)
        {
            var message = obj as PTMessage;
            return message != null &&
                   senderName == message.senderName &&
                   gameName == message.gameName;
        }
        public override int GetHashCode()
        {
            var hashCode = -1551645312;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(senderName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(gameName);
            return hashCode;
        }
        public static bool operator ==(PTMessage message1, PTMessage message2)
        {
            return EqualityComparer<PTMessage>.Default.Equals(message1, message2);
        }
        public static bool operator !=(PTMessage message1, PTMessage message2)
        {
            return !(message1 == message2);
        }

    }

    /*Session info message sent from Tabletop
     * , recieved and stored on Handheld side*/
    public class PTSession : PTMessage
    {
        public byte[] image;
        public string ip;
        public int port;
        //decors
        //public Decor[] decors

        public new static PTSession FromJson(string json)
        {
            try
            {
                return JsonUtility.FromJson<PTSession>(json);
            }
            catch
            {
                return null;
            }
        }

        public static PTSession Find(string senderName, string gameName)
        {
            foreach (PTSession session in PTManager.NearbySessions)
            {
                if (session.senderName == senderName && session.gameName == gameName)
                {
                    return session;
                }
            }
            return null;
        }
    }

    /*Player info message sent from Handheld
     * , recieved and stored on Tabletop side*/
    public class PTPlayer
    {
        public string name;
        public int connectionId;
        public Sprite image;

        public PTPlayer(string name, int connectionId, Sprite image)
        {
            this.name = name;
            this.connectionId = connectionId;
            this.image = image;
        }

        public static PTPlayer Find(string playerName)
        {
            foreach (PTPlayer player in PTManager.Players)
            {
                if (player.name == playerName)
                {
                    return player;
                }
            }
            return null;
        }
        public static PTPlayer Find(int connectionId)
        {
            foreach (PTPlayer player in PTManager.Players)
            {
                if (player.connectionId == connectionId)
                {
                    return player;
                }
            }
            return null;
        }

        public override bool Equals(object obj)
        {
            var other = obj as PTPlayer;
            return other != null &&
                   this.name == other.name;
        }

        public override int GetHashCode()
        {
            var hashCode = -1551645312;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(name.ToString());
            return hashCode;
        }

        public override string ToString()
        {
            return JsonUtility.ToJson(this);
        }
    }

    /*SmartPiece*/
    public class SmartPiece
    {
        //Smart piece parameters
        const string SP_RECEIVER_NAME = "com.prizm.unityreceiver.SPReceiver"; //<<DONT TOUCH
        const string SP_DATA_NAME = "spData";                                 //<<DONT TOUCH    
        const string SP_DATA_COORD_X = "xCoord";
        const string SP_DATA_COORD_Y = "yCoord";
        const string SP_DATA_UID = "uid";
        const string SP_DATA_FRESH = "fresh"; //whether it's just been pressed down onto the board vs it's stayed there
        const int DEFAULT_COORD = -1;

        //SmartPiece Receiver, receives data from SmartPiece Service for RFID scans
        static AndroidJavaClass spReceiver
#if UNITY_ANDROID && !UNITY_EDITOR
            = new AndroidJavaClass(SP_RECEIVER_NAME)
#endif
            ;
        //Structure describing smart piece data. From Android plugin, classes.jar { bool: fresh; string: uid; int: xCoord; int: yCoord }
        static AndroidJavaObject spData
#if UNITY_ANDROID && !UNITY_EDITOR
            = spReceiver.GetStatic<AndroidJavaObject>(SP_DATA_NAME)
#endif
            ;

        int _x;
        int _y;
        string _id;
        byte[] _data;
        public int x { get { return _x; } }
        public int y { get { return _y; } }
        public string id { get { return _id; } }
        public byte[] data { get { return _data; } }

        private SmartPiece(int x, int y, string id, byte[] data)
        {
            _x = x;
            _y = y;
            _id = id != null ? id : DateTime.UtcNow.ToString();
            _data = data;
        }

    //public SmartPiece(int x, int y, string id)
    //{
    //  _x = x;
    //  _y = y;
    //  _id = id != null ? id : DateTime.UtcNow.ToString();
    //  _data = null;
    //}

        /*API*/
        public static SmartPiece Get()
        {
            int recvX = DEFAULT_COORD;
            int recvY = DEFAULT_COORD;
            string recvUid = null;


            if (spData != null && spData.Get<bool>(SP_DATA_FRESH))
            {
                //Get coord
                try
                {
                    recvX = spData.Get<int>(SP_DATA_COORD_X);
                    recvY = spData.Get<int>(SP_DATA_COORD_Y);
                }
                catch
                {
                    return null;
                }

                //Get uid
                try
                {
                    recvUid = spData.Get<string>(SP_DATA_UID);
                }
                catch { }

                //Set fresh to be false
                spData.Set(SP_DATA_FRESH, false);
                return new SmartPiece(recvX, recvY, recvUid, null);
            }
            return null;

        }

        /*Override*/
        public override string ToString()
        {
            return "{\"x\"=" + _x
                + ", \"y\"=" + _y
                + ", \"id\"=\"" + _id + "\""
                //missing data to string
                + "}";
        }
        public override bool Equals(object obj)
        {
            var sp = obj as SmartPiece;
            return sp != null &&
                   x == sp.x &&
                   y == sp.y;
        }
        public override int GetHashCode()
        {
            var hashCode = -1551645312;
            hashCode = hashCode * -1521134295 + x;
            hashCode = hashCode * -1521134295 + y;
            return hashCode;
        }
        public static bool operator ==(SmartPiece sp1, SmartPiece sp2)
        {
            return EqualityComparer<SmartPiece>.Default.Equals(sp1, sp2);
        }
        public static bool operator !=(SmartPiece sp1, SmartPiece sp2)
        {
            return !(sp1 == sp2);
        }
    }
}

//TODO:
//ienumerator Send wait for receiver's confirmation 
//Send function also works between players
//Fix scene reloading bug!