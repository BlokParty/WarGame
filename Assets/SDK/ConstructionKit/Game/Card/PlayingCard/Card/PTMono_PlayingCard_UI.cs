using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayTable;
using UnityEngine.UI;
using System;

namespace PlayTable.Unity
{
    public class PTMono_PlayingCard_UI : PTUI
    {
        public Image imageSuit;
        public Sprite[] suitSprites;//0=Diamond, 1=Heart, 2=Spade, 3=Club
        public Text textFace;
        public GameObject center;
        PTTableTop_Playingcard rule;
        PTMono_PlayerTT monoPlayer;
        PTMono_PlayingcardHand monoHand;

        public Face Face
        {
            get
            {
                return card.face;
            }
        }
        public Suit Suit
        {
            get
            {
                return card.suit;
            }
        }

        private Playingcard card;

        private void Awake()
        {
            rule = FindObjectOfType<PTTableTop_Playingcard>();
            monoPlayer = GetComponentInParent<PTMono_PlayerTT>();
            monoHand = monoPlayer.GetComponentInChildren<PTMono_PlayingcardHand>();
            GetComponent<Button>().onClick.AddListener(OnClicked);
        }

        private void Start()
        {
            UpdateUI();
            StartCoroutine(SetPlayable());
        }

        private void OnClicked()
        {
            //trigger the event on Rule (customizable for developers)
            if (rule)
            {
                try
                {
                    monoHand.Play(card);
                }
                catch { }
            }
        }

        void UpdateUI()
        {
            if (card == null)
                return;

            imageSuit.sprite = suitSprites[(int)(card.suit)];
            string currFace = "";
            switch (card.face)
            {
                case Face.Ace: currFace = "A"; break;
                case Face.Jack: currFace = "J"; break;
                case Face.Queen: currFace = "Q"; break;
                case Face.King: currFace = "K"; break;
                default: currFace = ((int)card.face).ToString(); break;
            }
            textFace.text = currFace;
        }

        public void UpdateUI(Playingcard newCard)
        {
            card = newCard;
            UpdateUI();
        }

        public override string ToString()
        {
            return "face=" + card.face + " suit=" + card.suit;
        }

        IEnumerator SetPlayable()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();
                bool playable = rule ? rule.canPlay(monoPlayer, card) : true;
                GetComponent<Button>().interactable = playable;
            }
        }
    }
}
