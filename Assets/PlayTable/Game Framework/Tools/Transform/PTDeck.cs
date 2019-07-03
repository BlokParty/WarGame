using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayTable
{
    public class PTDeck : PTZone
    {
        protected void Awake()
        {
            OnAdded += (card, from) =>
            {
                StartCoroutine(ArrangeCoroutine(PT.DEFAULT_TIMER));
            };

            FlipTogether(false, true, PT.DEFAULT_TIMER);
            StartCoroutine(ArrangeCoroutine(PT.DEFAULT_TIMER));
        }

        public virtual void DealToAllPlayers(int amount)
        {
            foreach (PTPlayer player in PTTableTop.players)
            {
                PTZone currZone = player.GetComponent<PTZone>();
                Deal(currZone, amount, false);
            }
        }
        public virtual void Deal(PTZone zone, int amount, bool faceup)
        {
            for (int i = 0; i < amount; i++)
            {
                if (content.childCount > 0)
                {
                    Transform currCard = content.GetChild(0);
                    currCard.ToggleVisibility(true, PT.DEFAULT_TIMER);
                    currCard.Flip(faceup, PT.DEFAULT_TIMER);
                    zone.Add(currCard.transform, PT.DEFAULT_TIMER * 2);
                }
            }
            StartCoroutine(ArrangeCoroutine(PT.DEFAULT_TIMER));
        }
    }
}