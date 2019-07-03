using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayTable;

public class WarCardFactory : Singleton<WarCardFactory>
{

    private WarCardFactory[] cards = new WarCardFactory[52];
    private int cardsPerPlayer;
    private int deckSize = 52;
    private Vector3 boardPosition;
    public int cardNumber;
    public GameObject playerParent;

    [SerializeField]
    GameObject player, cardToSpawn;

    [SerializeField]
    GameObject cardPrefab, board, deck;

  //  parent = gameObject.transform;


    // Use this for initialization
    void Start ()
    {
        playerParent = PTTableTop.players[0].transform.parent.gameObject;
        spawnDeck();
        distributeCards();

    }
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    public void spawnDeck()
    {
        for(int i = 0;i< deckSize; ++i)
        {
            cardToSpawn = Instantiate(cardPrefab, deck.transform);
        }
    }

    public void distributeCards()
    {
       //Traverse over the deck until it's empty.
        while(deckSize>= 0)
        {
            //Distribute a card to each player
            foreach (PTPlayer player in WarRuleManager.orderOfPlayers)
            {   
                Transform selector = player.transform.GetChild(0);
                deckSize--;
                if (deckSize < 0)
                    return;
                //Give each player a card
                DealOneCard(selector);
            }
        }
    }

    public void DealOneCard(Transform selector)
    {
            //Set the parent of each card to the player's selector
            deck.transform.GetChild(0).parent = selector.transform;
    }
}
