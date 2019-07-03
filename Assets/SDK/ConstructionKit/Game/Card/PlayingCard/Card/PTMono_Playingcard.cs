using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayTable;

public class PTMono_Playingcard : PTMono_Touchable {
    public Sprite[] suitSprites;
    public Text face;
    public Image suit;

    private Playingcard card;
    public Playingcard Card
    {
        get
        {
            return card;
        }

        set
        {
            card = value;
        }
    }

    private void Awake()
    {
        transform.Find("Canvas").GetComponent<Canvas>().worldCamera = Camera.main;
    }

    public GameObject NewCardObject(Playingcard newCard)
    {
        GameObject newCardGameObject = Instantiate(gameObject);
        PTMono_Playingcard mono = newCardGameObject.GetComponent<PTMono_Playingcard>();
        mono.UpdateContent(newCard);
        return newCardGameObject;
    }
    public void UpdateContent()
    {
        if (card == null)
            return;

        string textFace = "";
        switch (card.face)
        {
            case Face.Ace: textFace = "A"; break;
            case Face.Jack: textFace = "J"; break;
            case Face.Queen: textFace = "Q"; break;
            case Face.King: textFace = "K"; break;
            default: textFace = ((int)card.face).ToString(); break;
        }

        face.text = textFace;
        suit.sprite = suitSprites[(int)card.suit];
    }
    public void UpdateContent(Playingcard newCard)
    {
        card = newCard;
        UpdateContent();
    }

    protected override void OnTouchDown()
    {
    }

    protected override void OnTouchDrag()
    {
    }

    protected override void OnTouchEnter()
    {
    }

    protected override void OnTouchExit()
    {
    }

    protected override void OnTouchUp()
    {
    }
}
