/**How to use PlayTable Unity SDK
 * 
 * Create a new empty Game Object(Optional: rename it as TableTop or Handheld)
 * Add a new script component to the object. Open it.
 * Add namespace: using PlayTable.Unity;
 * Change parent class from MonoBehaviour to PTTableTop
 * Delete Start() and Update();
 * Right click on the red-lined user-defined new class name
 * Click Quick Actions and Refactorings...
 * Click Implement Abstract classes
 * Go back to unity editor and drag the required prefabs, canvasSetting for example, to the script.
 * Change the default throwing NotImplementedException behavior to your own behaviors
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace PlayTable.Unity
{
    public abstract class PTTableTop : MonoBehaviour
    {
        public PTMono_PlayerTT ptMonoPlayer;
        public PTMono_CanvasSettings canvasSettings;
        public PTMono_CanvasConnect canvasConnect;
        public bool autoCreatePlayer = true;
        [HideInInspector]
        public int botCount = 0;
        protected PTMono_PlayerTT[] players
        {
            get
            {
                return FindObjectsOfType<PTMono_PlayerTT>();
            }
        }
        protected PTMono_PlayerTT currPlayer;

        const float X_MIN = -9f;
        const float X_MAX = 9f;
        const float Z_MIN = -5f;
        const float Z_MAX = 5f;

        protected virtual void Awake()
        {
            //init Event system
            if (!FindObjectOfType<EventSystem>())
            {
                GameObject objectEventSys = new GameObject();
                objectEventSys.name = "Event System";
                objectEventSys.AddComponent<EventSystem>();
                objectEventSys.AddComponent<StandaloneInputModule>();
            }

            //init canvasSettings
            PTMono_CanvasSettings existingCanvasSetting = FindObjectOfType<PTMono_CanvasSettings>();
            if (!existingCanvasSetting)
            {
                existingCanvasSetting = Instantiate(canvasSettings.gameObject).GetComponent<PTMono_CanvasSettings>();
                canvasSettings = existingCanvasSetting;
            }

            //init canvasConnect
            PTMono_CanvasConnect existingCanvasConnect = FindObjectOfType<PTMono_CanvasConnect>();
            if (!existingCanvasConnect)
            {
                existingCanvasConnect = Instantiate(canvasConnect.gameObject).GetComponent<PTMono_CanvasConnect>();
                canvasConnect = existingCanvasConnect;
            }
        }
        protected virtual void Start()
        {
            StartCoroutine(Rule());
            StartCoroutine(UpdatePlayersInfo());
        }

        /*main rule*/
        private IEnumerator Rule()
        {
            while (true)
            {
                initSettingCanvas();
                //Includes network, rules, player nums, purchasing, customizing, decor...
                yield return Settings();
                //Get ready
                yield return Connect();
                //Play Game
                yield return Game();
                yield return new WaitUntil(()=> Game_IsEnd());
            }
        }


        //Settings
        private void initSettingCanvas()
        {
            foreach (Canvas canvas in FindObjectsOfType<Canvas>())
            {
                canvas.gameObject.SetActive(false);
            }
            canvasSettings.gameObject.SetActive(true);
        }
        private IEnumerator Settings()
        {
            Settings_OnStart();
            yield return new WaitUntil(() => Settings_IsEnd());
        }
        protected abstract IEnumerator Settings_OnStart();
        private bool Settings_IsEnd()
        {
            return !canvasSettings.gameObject.activeSelf;
        }

        //Connect
        private IEnumerator Connect()
        {
            //Setup PTManager
            PTManager.Initialize(true);
            //Start accepting players, wait until player num reached the min requirement
            yield return Ready();
            //Display all players
            if (autoCreatePlayer)
            {
                yield return DisplayPlayers();
            }
        }
        private IEnumerator Ready()
        {
            canvasConnect.gameObject.SetActive(true);
            print(PTManager.singleton);
            yield return new WaitUntil(() =>
            {
                foreach (PTPlayer player in PTManager.Players)
                {
                    if (!PTManager.isReady(player))
                    {
                        return false;
                    }
                }
                return true;
            });
            yield return new WaitUntil(() => !canvasConnect.gameObject.activeSelf);

        }
        private IEnumerator DisplayPlayers()
        {

            /*//Auto max player
             * float timer = 1f;
             * Dictionary<PTPlayer, bool>.KeyCollection.Enumerator ptPlayerEnum = PTManager.Players.GetEnumerator();
             * for (int i = 0; i < PTManager.Players.Count; i++)
            {
                yield return new WaitForSeconds(timer);
                PTPlayer ptPlayer = ptPlayerEnum.MoveNext() ? ptPlayerEnum.Current : null;
                if (ptPlayer != null || autoMaxPlayer)
                {
                    yield return InstantiatePlayerTT(ptPlayer);
                }
            }*/

            foreach (PTPlayer player in PTManager.Players)
            {
                yield return InstantiatePlayerTT(player);
            }

            for (int i = 0; i < botCount; i++)
            {
                yield return InstantiatePlayerTT(null);
            }
        }
        private IEnumerator InstantiatePlayerTT(PTPlayer ptPlayer)
        {
            //paramaters
            float awayFromCenter = 0.6f;
            float currY = 7.5f;

            //random init position
            Vector3 currPos = Vector3.zero;
            while (true)
            {
                yield return new WaitForEndOfFrame();
                float currX = UnityEngine.Random.Range(X_MIN, X_MAX);
                float currZ = UnityEngine.Random.Range(Z_MIN, Z_MAX);
                if ((currX > X_MAX * awayFromCenter || currX < X_MIN * awayFromCenter)
                    && (currZ > Z_MAX * awayFromCenter || currZ < Z_MIN * awayFromCenter))
                {
                    currPos = new Vector3(currX, currY, currZ);
                    break;
                }
            }

            //instantiate game object
            PTMono_PlayerTT newPlayer = Instantiate(ptMonoPlayer.gameObject, currPos, ptMonoPlayer.transform.rotation).GetComponent<PTMono_PlayerTT>();

            //Assign the ptPlayer to the new player game object
            newPlayer.ptPlayer = ptPlayer;

            //Set name
            int countPresetName = Enum.GetNames(typeof(PresetName)).Length;
            newPlayer.name = newPlayer.ptPlayer == null ? General.RandName() + "(Bot)" : newPlayer.ptPlayer.name;
            while (true)
            {
                yield return new WaitForEndOfFrame();

                bool hasSameName = false;
                foreach (PTMono_PlayerTT player in players)
                {
                    if (!player.Equals(newPlayer) && player.name == newPlayer.name)
                    {
                        hasSameName = true;
                        print("while InstantiatePlayerTT, same name detected:" + player.name);
                        break;
                    }
                }
                if (hasSameName)
                    newPlayer.name = newPlayer.ptPlayer == null ? General.RandName() + "(Bot)" : newPlayer.ptPlayer.name;
                else
                    break;
            }

            //wait until newplayer is instantiated successfully
            yield return new WaitUntil(() => newPlayer != null);
        }

        //Update Players info
        private IEnumerator UpdatePlayersInfo()
        {
            float timer = 1f;
            while (true)
            {
                yield return new WaitForSeconds(timer);
                foreach (PTMono_PlayerTT player in players)
                {
                    yield return UpdatePlayerInfo(player);
                }
            }
        }
        protected abstract IEnumerator UpdatePlayerInfo(PTMono_PlayerTT player);

        //Game
        protected abstract IEnumerator Game();
        protected abstract bool Game_IsEnd();

    }

}

