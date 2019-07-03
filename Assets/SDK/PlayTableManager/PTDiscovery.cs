using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace PlayTable.Unity
{
    public class PTDiscovery : NetworkDiscovery
    {
        public override void OnReceivedBroadcast(string fromAddress, string data)
        {
            //Corner case
            if (PTManager.singleton == null)
            {
                return;
            }

            //Get the new session by received broadcast data
            string serverAddress = fromAddress.Contains("::ffff:") ? 
                fromAddress.Substring(7) : fromAddress;
            try
            {
                //New the PTBroadcastMessage
                //print(data);
                PTSession session = PTSession.FromJson(data);
                //print(session);

                session.ip = serverAddress;


                //invoke DiscoveredSession on PTHandHeld
                if (PTManager.OnBroadcastReceieved != null)
                {
                    PTManager.OnBroadcastReceieved(session);
                }
            }
            catch (ArgumentException e)
            {
                Debug.LogError(e);
            }
            catch (Exception)
            {
                //Debug.LogError("Unknown Error: OnReceivedBroadcast: " + data);
            }
        }
    }
}
