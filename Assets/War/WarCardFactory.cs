using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayTable;

public class WarCardFactory : Singleton<WarCardFactory>
{
    private int cardsPerPlayer;
    private int deckSize = 52;
    private Vector3 boardPosition;

    public int cardNumber;
    public GameObject playerParent;

    [SerializeField]
    GameObject cardToSpawn;

    [SerializeField]
    GameObject cardPrefab, board, deck;

    
    private const int cardMaxValue = 13;
    private const int numberOfSuits = 4; 


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
            //Spawn all cards, set each card's parent to the deck game object
           // cardToSpawn = Instantiate(cardPrefab, deck.transform);
        }

        for (int i = 0; i < cardMaxValue; ++i)
        {
            for (int j = 0; j < numberOfSuits; ++j)
            {

                //Spawn all cards, set each card's parent to the deck game object
                cardToSpawn = Instantiate(cardPrefab, deck.transform);
                cardToSpawn.transform.GetComponent<WarCard>().value = i;
                cardToSpawn.transform.GetComponent<WarCard>().suit = (Suit)j;

            }
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
