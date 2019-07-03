using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using PlayTable;
using PlayTable.Unity;
using UnityEngine.UI;
using System.Text;

namespace PlayTable.Unity
{

    public class PTMono_PlayerTT : PTMono_Touchable
    {
        public PTPlayer ptPlayer;
        public PTMono_3DButton buttonCanvas;
        public PTMono_PlayingcardHand playingcardHand;
        public GameObject panelMain;
        public Text textInfo;
        public Text textName;
        public Transform nav;
        public string team = "";

        bool uiVisible = true;

        internal virtual void Awake()
        {
            buttonCanvas.OnClicked += OnButtonCanvasClicked;
        }

        internal virtual void Start()
        {
            StartCoroutine(LookAtCenter());
            StartCoroutine(SyncHandheld());
            Fall();
        }

        internal virtual void Update()
        {
            textName.text = name;
        }

        private IEnumerator SyncHandheld()
        {

            float refreshTimer = 1;
            while (true)
            {
                if (ptPlayer == null)
                {
                    yield return new WaitForSeconds(2 * refreshTimer);
                }
                else
                {
                    yield return new WaitForSeconds(refreshTimer);
                    yield return SyncPlayingcardHands();
                    yield return SyncPieces();
                }
            }
        }
        private IEnumerator SyncPieces()
        {
            yield return new WaitForEndOfFrame();
        }
        private IEnumerator SyncPlayingcardHands()
        {
            yield return new WaitForEndOfFrame();
        }
        private IEnumerator LookAtCenter()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();
                transform.LookAt(new Vector3(0, transform.position.y, 0), Vector3.up);
            }
        }
        protected override void OnTouchDown()
        {
        }
        protected override void OnTouchExit()
        {
        }
        protected override void OnTouchEnter()
        {
        }
        protected override void OnTouchUp()
        {
        }
        protected override void OnTouchDrag()
        {
        }
        public IEnumerator ActAs(Mood mood)
        {
            print(name + " " + mood);
            yield return new WaitForSeconds(1);
        }
        private void OnButtonCanvasClicked(PTMono_3DButton button)
        {
            uiVisible = !uiVisible;

            //Toggle canvas vis
            playingcardHand.gameObject.SetActive(uiVisible);
            textInfo.gameObject.SetActive(uiVisible);
            textInfo.gameObject.SetActive(uiVisible);
            nav.gameObject.SetActive(uiVisible);

            //Change button text
            button.GetComponentInChildren<Text>().text = uiVisible ? "Hide" : "Show";
        }
    }

}