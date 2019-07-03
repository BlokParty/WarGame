using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Text;

/** \brief Classes available for use in Unity dev environment
 * 
 * 
 */
namespace PlayTable.Unity
{

    /** \brief Attach this to a Game Manager object
     * 
     * handles networking bootstrap; hosts network for conneced clients
     * automatically sends broadcasts to sync with handheld clients
     * 
     * 
     */
    public sealed class PTGameManager : NetworkLobbyManager
    {
        public static new PTGameManager singleton { get { return NetworkLobbyManager.singleton.GetComponent<PTGameManager>(); } }
        public NetworkDiscovery networkFinder;

        private Broadcasts broadcasts = new Broadcasts();

        public Broadcasts Broadcasts
        {
            get
            {
                if (!networkFinder.running)
                    return null;
                foreach (KeyValuePair<string, NetworkBroadcastResult> bPair in networkFinder.broadcastsReceived)
                {
                    broadcasts.Add(new Unity.Broadcast(bPair.Value));
                }
                return broadcasts;
            }
        }

        /**
         * true if device running is PlayTable
         * 
         */
        public bool isTableTop;
        public bool IS_TABLETOP
        {
            //Simply assume the device is PlayTable if the platform is Android
            get
            {
                return isTableTop;
                //return Application.platform == RuntimePlatform.Android;
            }
        }

        /** \brief Opens the server to be connected with clients
         * 
         * The flow for starting a game is:
         * 1) StartNetwork() - Allow handheld clients to connect (during a lobby for instance)
         * 2) StopNetwork() - Stop further players from joining a game (Allow max players for instance)
         * 3) StartGame() - changes scene to game play and starts the server for in-game broadcasts
         * 
         * 
         * *Automaticaly detects if on PlayTable (Tabletop) or not (Handheld)
         * Should be called on both Tabletop game and Handheld game
         * 
         */
        public void StartNetwork()
        {
            if (isNetworkActive)
                return;
            if (IS_TABLETOP)
            {
                StartServer();
                StartBroadcast();
            }
            else
            {
                StartListen();
            }
        }

        /** \brief Call this to prevent further clients from connecting
         * 
         * Prevents Handheld clients from connecting to server
         * 
         * Should call this at the end of the lobby matchmaking sequence,
         * at the end of the game, 
         * and in between rounds where players can't connect 
         * 
         */
        public void StopNetwork()
        {
            if (IS_TABLETOP)
            {
                StopServer();
            }
            else
            {
                StopClient();
            }
            StopNetworkFinder();
        }

        /** 
         * Changes the scene to the PlayScene
         * 
         * Starts in-game networking for broadcasts to transmit data
         * 
         * 
         */
        public void StartGame()
        {
            if (!isNetworkActive)
            {
                if (StartServer())
                {
                    ServerChangeScene(playScene);
                }
            }
            else
            {
                ServerChangeScene(playScene);
            }

        }

        private bool SpendPoint(int amount) { throw new NotImplementedException(); }


        //these functions are not implemented yet
        //TODO:Implement and take off HIDDEN_SYMBOLS condition to document
        /** \cond HIDDEN_SYMBOLS */

        /** \brief Buy is not implemented yet!
         * 
         * Buying is not implemented yet!
         * Buys something from the PlayTable store
         * Spends a player's connected in-game currency (PT Points)
         * Throws an exception if the player does not have enough money
         * Developer can take exception to prompt player to take action or send a notification
         * 
         * @param itemID itemID is the thing
         * 
         */
        public bool Buy(int itemID) { throw new NotImplementedException(); }

        /** \brief Gift is not implemented yet! 
         * 
         * 
         * Allows a User to gift an object to another User
         * 
         * @param receiverID The receiver's PT User ID
         * @param pointAmount How many points to give to User's PT account
         * 
         */
        public bool Gift(int receiverID, int pointAmount) { throw new NotImplementedException(); }

        /** \brief GainPoints is not implemented yet!
         * 
         * Used to gain exp for that particular game
         * ex. configure game rewards so that achievement is unlocked @ 500 points
         * 
         * 
         * @param amount how many points to gain
         * 
         */
        public bool GainPoints(int amount) { throw new NotImplementedException(); }

        /** \brief ShowTutorials is not implemented yet!
         * 
         * 
         * 
         * Call this to let PT servers know this player is getting tutorials shown to them
         * Can make it so that a player automatically gets tutorials for 1st time and gets rewards for completing it
         * 
         * 
         */
        public void ShowTutorials() { throw new NotImplementedException(); }

        /** \brief LoadObject is not implemented yet!
         * 
         * 
         * Load an object for use in game
         * ex: DL assets from PT servers
         * 
         * 
         * @param dir directory to load assets from
         * 
         * 
         */
        public T LoadObject<T>(string dir) { throw new NotImplementedException(); }

        /** \brief Topup is not implemented yet!
         * 
         * Have the player buy PT Currency
         * 
         * 
         * @param usdAmount amount in US Dollars.  ex. 100 = $100
         * 
         */
        public bool Topup(int usdAmount) { throw new NotImplementedException(); }


        void InitNetworkDiscovery()
        {
            //Find network component
            /*if (PTNetworkPrefab  == NotFiniteNumberException)
            {
                Debug.LogError("PTNetwork prefab not set");
            }*/

            //Instantiate new GameObject PTNetworkIntance
            if (!GetComponent<NetworkDiscovery>())
            {
                networkFinder = gameObject.AddComponent<NetworkDiscovery>();
            }

            //Set Network scenes
            if (lobbyScene == null || lobbyScene == null)
            {
                Debug.Log("Scenes not set");
            }

            //Set network player prefabs
            if (!lobbyPlayerPrefab || !gamePlayerPrefab)
            {
                Debug.Log("Player prefabs not set");
            }

            //Misc   
            //lobbyManager.InitGameFinder();
            showLobbyGUI = false;

            if (networkFinder)
            {
                networkFinder.showGUI = false;
                networkFinder.offsetX = 300;
            }
        }

        public override void OnClientConnect(NetworkConnection conn)
        {
            base.OnClientConnect(conn);
        }

        void StartBroadcast()
        {
            StartCoroutine(Broadcast());
        }

        IEnumerator Broadcast()
        {
            yield return new WaitUntil(() => networkFinder);
            if (networkFinder.running)
            {
                networkFinder.StopBroadcast();
            }
            yield return new WaitUntil(() => networkFinder.Initialize());
            networkFinder.broadcastData = SystemInfo.deviceName;
            networkFinder.StartAsServer();
        }

        void StartListen()
        {
            StartCoroutine(Listen());
        }

        IEnumerator Listen()
        {
            yield return new WaitUntil(() => networkFinder);
            if (networkFinder.running)
            {
                networkFinder.StopBroadcast();
            }
            yield return new WaitUntil(() => networkFinder.Initialize());
            networkFinder.StartAsClient();
        }

        void StopNetworkFinder()
        {
            if (networkFinder.running)
                networkFinder.StopBroadcast();
        }

        private IEnumerator BroadcastGarbageClean()
        {
            while (true)
            {
                yield return new WaitForSeconds(1);
                for (int i = 0; i < broadcasts.Content.Count; i++)
                {
                    if (broadcasts.Content[i].Life <= 0)
                        broadcasts.RemoveAt(i);
                }
            }
        }

        private IEnumerator BroadcastTimeout()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();
                foreach (Broadcast broadcast in broadcasts.Content)
                {
                    broadcast.TimeoutByDeltatime();
                }
            }
        }

        public void StartConnectServer(Broadcast b)
        {
            networkAddress = b.serverAddress;
            StartClient();
        }

        private void Start()
        {
            InitNetworkDiscovery();
            StartCoroutine(BroadcastTimeout());
            StartCoroutine(BroadcastGarbageClean());
        }
    }

    /** \brief The main mechanism by which handhelds communicate with tabletop
* 
* 
* 
*/
    public class Broadcast
    {

        /**
         * \brief Where the broadcast is from
         * 
         */
        public string serverAddress;

        /**
         * \brief Data received with the broadcast
         * 
         */
        public string data;

        /**
         * \brief Timeout of each broadcast
         * 
         */
        const float INIT_LIFE = 5;

        /**
         * \brief Time left for a broadcast before destroy
         * 
         */
        private float life = INIT_LIFE; public float Life { get { return life; } }

        /**
        * \brief Constructor of a broadcast
        * 
        * @param f fromAddress
        * @param d datas - string to be sent.
        *   data can be defined by the developer.  JSON string, etc.
        * 
        */
        public Broadcast(string f, string d)
        {
            serverAddress = f;
            data = d;
        }

        /**
         * \brief Constructor of a broadcast
         * 
         * @param nbr NetworkBroadcastResult
         * 
         */
        public Broadcast(NetworkBroadcastResult nbr)
        {
            serverAddress = nbr.serverAddress.Contains("::ffff:") ?
                nbr.serverAddress.Substring(7) : nbr.serverAddress;
            data = System.Text.Encoding.Unicode.GetString(nbr.broadcastData);
        }

        /**
        * \brief Reset the life of the broadcast to its initial life
        * 
        */
        public void ResetLife()
        {
            life = INIT_LIFE;
        }

        /**
        * Deduct the life of a broadcast by Time.deltaTime
        * (the time in seconds it took to complete the last frame)
        * 
        */
        public void TimeoutByDeltatime()
        {
            life -= Time.deltaTime;
        }


    }

    /** \brief for sending multiple broadcasts to all connected clients simultaneously
     * 
     * Loads a list of Broadcast and can retrieve them
     */
    public class Broadcasts
    {
        List<Broadcast> content = new List<Broadcast>();
        public List<Broadcast> Content { get { return content; } }

        /** \brief Add the Broadcast to the list
         * 
         * functions like a List<>()
         * 
         */
        public bool Add(Broadcast newBroadcast)
        {
            Broadcast existingB = GetBroadcast(newBroadcast.serverAddress);
            if (existingB != null)
            {
                existingB.ResetLife();
                return false;
            }
            else
            {
                content.Add(newBroadcast);
                return true;
            }
        }

        /** \brief Remove the Broadcast from the list
         * 
         * functions like a List<>()
         * 
         */
        public bool RemoveAt(int index)
        {
            if (index >= 0 && index < content.Count)
            {
                content.RemoveAt(index);
                return true;
            }
            return false;
        }

        /** \brief Retrieves a Broadcast from the list
         * 
         * functions like a List<>()
         * 
         * @param address developer should keep track of the addresses needed
         * 
         */
        Broadcast GetBroadcast(string address)
        {
            foreach (Broadcast b in content)
            {
                if (b.serverAddress == address)
                    return b;
            }
            return null;
        }
    }

}


