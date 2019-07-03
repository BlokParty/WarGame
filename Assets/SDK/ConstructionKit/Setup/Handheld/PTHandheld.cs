/**How to use PlayTable Unity SDK
 * 
 * very similar to PTTableTop.cs 
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayTable.Unity;
using UnityEngine.EventSystems;

namespace PlayTable.Unity
{
    public abstract class PTHandheld : MonoBehaviour
    {
        public PTMono_CanvasConnect canvasConnect;
        public PTMono_CanvasStore canvasStore;
        public PTMono_CanvasHandheld canvasHandheld;

        public delegate void PTPlayingCardUIDelegate(PTMono_PlayingCard_UI card);
        public PTPlayingCardUIDelegate OnPlayingcardUIClicked;

        protected virtual void Awake()
        {
            //Register delegates
            PTManager.OnConnected += Handler_OnConnected;
            PTManager.OnDisconnected += Restart;
            PTManager.OnDisconnected += Handler_OnDisconnected;
            PTManager.OnDataReceived += Handler_OnDataReceived;
            PTManager.OnReadyReceived += Handler_OnReadyRecieved;

            //init Event system
            if (!FindObjectOfType<EventSystem>())
            {
                GameObject objectEventSys = new GameObject();
                objectEventSys.name = "Event System";
                objectEventSys.AddComponent<EventSystem>();
                objectEventSys.AddComponent<StandaloneInputModule>();
            }

            //init canvasConnect
            PTMono_CanvasConnect existingCanvasConnect = FindObjectOfType<PTMono_CanvasConnect>();
            if (!existingCanvasConnect)
            {
                existingCanvasConnect = Instantiate(canvasConnect.gameObject).GetComponent<PTMono_CanvasConnect>();
                canvasConnect = existingCanvasConnect;
            }

            //init canvasStore
            PTMono_CanvasStore existingCanvasStore = FindObjectOfType<PTMono_CanvasStore>();
            if (!existingCanvasStore)
            {
                existingCanvasStore = Instantiate(canvasStore.gameObject).GetComponent<PTMono_CanvasStore>();
                canvasStore = existingCanvasStore;
            }
        }
        protected virtual void Start()
        {
            Restart(null);
        }

        IEnumerator Rule()
        {
            //Setup PTManager
            PTManager.Initialize(false);

            yield return Connect();
            while (true)
            {
                yield return Store();
                yield return Game();
            }
        }
        private IEnumerator Connect()
        {
            //Connect to nearby session
            canvasConnect.gameObject.SetActive(true);
            canvasConnect.StartCoroutine(canvasConnect.UpdateContent());
            canvasStore.gameObject.SetActive(false);
            canvasHandheld.gameObject.SetActive(false);
            yield return new WaitUntil(() => !canvasConnect.gameObject.activeSelf);
        }
        private IEnumerator Store()
        {
            //Browse store then get ready
            canvasStore.gameObject.SetActive(true);
            canvasConnect.gameObject.SetActive(false);
            canvasHandheld.gameObject.SetActive(false);
            yield return new WaitUntil(() => !canvasStore.gameObject.activeSelf);
        }
        private IEnumerator Game()
        {
            canvasConnect.gameObject.SetActive(false);
            canvasStore.gameObject.SetActive(false);
            canvasHandheld.gameObject.SetActive(true);
            yield return new WaitUntil(() => !canvasHandheld.gameObject.activeSelf);
        }

        private void Restart(PTMessage msg)
        {
            StopAllCoroutines();
            StartCoroutine(Rule());
        }
        private void Handler_OnReadyRecieved(PTMessage ptMsg)
        {
            canvasStore.gameObject.SetActive(false);
        }

        protected abstract void Handler_OnConnected(PTMessage msg);
        protected abstract void Handler_OnDisconnected(PTMessage msg);
        protected abstract void Handler_OnDataReceived(PTMessage msg);
    }

}
