using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayTable;

public class PTMono_PlayingcardDeck : PTMono_Touchable
{
    public int numPlayers;
    public PTMono_Playingcard ptPlayingcard;
    private Deck_Playingcard deck = new Deck_Playingcard();
    public int Remain { get { return deck.remain; } }

    public void Shuffle()
    {
        deck.Shuffle();
    }
    public Playingcard Deal()
    {
        Playingcard ret = (Playingcard)deck.Deal();
        if (Remain <= 0)
        {
            try
            {
                Destroy(gameObject);

            }
            catch { }
        }
        return ret;
    }
    public GameObject DealGameObject()
    {
        GameObject ret = ptPlayingcard.NewCardObject((Playingcard)deck.Deal());
        if (Remain <= 0)
        {
            try
            {
                Destroy(gameObject);

            }
            catch { }
        }
        return ret;
    }
    public void Add (Playingcard card)
    {
        deck.Add(card);
    }
    public void AddStandardCards()
    {
        deck.AddStandardCards();
    }
    public void AddJokers()
    {
        deck.AddJokers();
    }

    private void OnTriggerEnter(Collider other)
    {

    }
    private void OnTriggerExit(Collider other)
    {
        
    }
    private void OnTriggerStay(Collider other)
    {

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
}
