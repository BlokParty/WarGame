using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayTable;

public class PTMono_PlayingcardPile : PTMono_CardCollection_Playingcard
{
    private void Awake()
    {
        cards = new Pile_Playingcard();
    }

    private void OnTriggerEnter(Collider other)
    {
        PTMono_Playingcard playingCard = other.GetComponent<PTMono_Playingcard>();
        if (playingCard)
        {
            cards.Add(playingCard.Card);
            Destroy(other.gameObject);
            UpdateContent();
        }
    }
}
