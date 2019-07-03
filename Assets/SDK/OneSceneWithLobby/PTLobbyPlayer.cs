using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace PlayTable.Unity
{
    /**
     * \brief PTLobbyPlayer represents players in lobby matching
     * 
     * 
     */
    public sealed class PTLobbyPlayer : NetworkLobbyPlayer
    {
        void AddNetworkIdentity()
        {
            if (!GetComponent<NetworkIdentity>())
            {
                gameObject.AddComponent<NetworkIdentity>();
            }
            ShowLobbyGUI = false;
        }

        /** \brief Adds the network identity
         * 
         */
        private void Awake()
        {
            AddNetworkIdentity();
        }

    }
}

