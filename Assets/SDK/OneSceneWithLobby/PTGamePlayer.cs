using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

namespace PlayTable.Unity
{
    /**
     * \brief All in-game players that require network sync should inherent from PTGamePlayer
     * 
     * Type of Network Behavior
     * 
     * Automatically handles network syncing
     * 
     */
    public sealed class PTGamePlayer : NetworkBehaviour
    {
        public NetworkTransform.TransformSyncMode sync;

        /* \cond HIDDEN_SYMBOLS */
        /**
         * \brief PlayerUpdate
         * 
         * NOT IMPLEMENTED
         * 
         */
        public bool PlayerUpdate<T>(T field) { throw new NotImplementedException(); }

        /**
         * \brief PlayerUpdate
         * 
         * NOT IMPLEMENTED
         * 
         */
        public bool PlayerUpdate(Event e) { throw new NotImplementedException(); }

        /* \endcond */


        /** \brief Main Delegate for receiving data
         * 
         * 
         * 
         * @param fromPlayer reference from the connected client that sent data
         * @param data string defined by developer to send
         * ex: json, game state string, etc.
         * 
         */
        public delegate void EventFromPlayerDelegate(PTGamePlayer fromPlayer, string data);


        [SyncEvent] public event EventFromPlayerDelegate EventFromPlayer;

        /** \brief Sends commands to the main PlayTable Host
         * 
         * 
         * @param data string defined by developer to send
         * ex: json, game state string, etc.
         */
        [Command]public void CmdSendEvent(string data){
            EventFromPlayer(this, data);
        }

        void AddNetworkComponents()
        {
            /*AddNetworkIdentity*/
            if (!GetComponent<NetworkIdentity>())
                gameObject.AddComponent<NetworkIdentity>();
            //make sure that the player is only local authorized
            GetComponent<NetworkIdentity>().localPlayerAuthority = true;

            /*AddNetworkTransform*/
            if (!GetComponent<NetworkTransform>())
            {
                gameObject.AddComponent<NetworkTransform>();
            }
            GetComponent<NetworkTransform>().transformSyncMode = sync;
        }

        /*IEnumerator PlaySceneInit()
        {
            int numPlayers = 0;
            foreach (PTLobbyPlayer player in PTGameManager.singleton.lobbySlots)
            {
                if (player)
                    numPlayers++;
            }

            yield return new WaitUntil(() => FindObjectsOfType<PTGamePlayer>().Length == numPlayers);
            PTGamePlayer[] gamePlayers = FindObjectsOfType<PTGamePlayer>();

            //Add EventFromPlayer to all players including the sender
            foreach (PTGamePlayer player in gamePlayers)
            {
                EventFromPlayer += player.OnEventFromPlayerReceived;
            }
            //Add event to event listeners for each player
            EventFromPlayer += OnEventFromPlayerReceived;
        }*/

        private void Start()
        {
            AddNetworkComponents();
            //StartCoroutine(PlaySceneInit());
        }
        
    }
}
