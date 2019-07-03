using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayTable;
using PlayTable.Unity;

public class PTMono_PlayingcardHand : PTMono_CardCollection_Playingcard
{
    public PTMono_PlayerTT player;
    public PTMono_PlayingcardPile pile;
    PTTableTop_Playingcard rule;



    public void Awake()
    {
        rule = FindObjectOfType<PTTableTop_Playingcard>();
        cards = new Hand_Playingcard();
    }

    public void Sort() { cards.Sort(); }
    public void Play(Playingcard card)
    {
        float cardHeight = 5;
        float transitionTimer = 1.5f;

        cards.Discard(card);
        GameObject newCard = card3D.NewCardObject(card);
        newCard.transform.position = new Vector3(0, cardHeight, 0) + transform.position;
        newCard.transform.SetParent (pile.transform);
        newCard.transform.localEulerAngles = new Vector3(0,0,0);
        //iTween.MoveTo(newCard, pile.transform.position, transitionTimer);
        UpdateContent();

        rule.OnPlayerPlayedCard(player, card);

    }
    public void Add(Playingcard card)
    {
        if (cards.Add(card))
        {
            UpdateContent();
        }
    }
    public void Draw(PTMono_PlayingcardDeck deck)
    {
        Add(deck.Deal());
    }
}
