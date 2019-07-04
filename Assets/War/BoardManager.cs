using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayTable;

public class BoardManager : PTZone {

    [SerializeField]
    WarRuleManager warRuleManager;


    private Dictionary<WarPlayer, Card> acceptedSubmissions = new Dictionary<WarPlayer, Card>();

    protected void Awake()
    {
       
        OnDropped += (PTTouchFollower touch) =>
        {
           // Debug.Log("working");
        };


        GetComponent<PTLocalInput>().OnTouchEnd_EndOnThis += (PTTouch touch) =>
        {
            Debug.Log("working");
            Debug.Log("Player : " + warRuleManager.currentPlayer.name);

        };


    }
    // Use this for initialization
    void Start () {
	    
	}
	
	// Update is called once per frame
	void Update ()
    {
       

    }

    public void CardFlipped()
    {
        //Insert into dictionary here

    }

    public void CardAccepted()
    {

    }


    public void DetermineHighestCard()
    {
        //Traverse all cards placed, player with highest card gains a single point
    }


}
