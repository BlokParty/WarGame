using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayTable;
using PlayTable.Unity;

public abstract class PTMono_CardCollection : MonoBehaviour {
    protected Collection_Card cards;
    public Collection_Card Cards
    {
        get
        {
            return cards;
        }
    }
    public int Count { get { return cards.Count; } }
    public override string ToString()
    {
        return JsonUtility.ToJson(this);
    }
    public void Discard(Card card)
    {
        cards.Discard(card);
        UpdateContent();
    }
    public void DiscardAll()
    {
        cards.DiscardAll();
        UpdateContent();
    }
    public Card Get(Card card)
    {
        return cards.Get(card);
    }
    public abstract void UpdateContent();
}

public abstract class PTMono_CardCollection_Playingcard : PTMono_CardCollection
{
    public PTMono_PlayingCard_UI cardUI;
    public PTMono_Playingcard card3D;
    Vector3 cardLocalScale = new Vector3(0.5f, 0.5f, 1);

    public bool hasSuit(Suit suit)
    {
        foreach (Playingcard card in cards.Cards)
        {
            if (card.suit == suit)
            {
                return true;
            }
        }
        return false;
    }
    public bool hasFace(Face face)
    {
        foreach (Playingcard card in cards.Cards)
        {
            if (card.face == face)
            {
                return true;
            }
        }
        return false;
    }

    public override void UpdateContent()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Playingcard card in cards.Cards)
        {
            GameObject newCard = Instantiate(cardUI.gameObject, transform);
            newCard.transform.localScale = cardLocalScale;
            newCard.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
            newCard.GetComponent<RectTransform>().anchoredPosition = new Vector2(0.5f, 1);

            newCard.GetComponent<PTMono_PlayingCard_UI>().UpdateUI(card);
        }
    }
}