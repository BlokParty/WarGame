using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayTable;
using System;

public class WarRuleManager : PTRuleManager_TurnBased
{

    WarPlayer[] warPlayers = new WarPlayer[8];
    public PTPlayer currentPlayer;


    public bool someoneHasWon = false;
    public int numberOfPasses;
    public bool canStart, canEnd, canRestart;

    protected override void Awake()
    {
        
        base.Awake();

        int activePlayerCount = GetComponent<WarCardFactory>().playerParent.transform.childCount;

        for (int i = 0;i<activePlayerCount; i++)
        {
            PTPlayer playerToAdd = GetComponent<WarCardFactory>().playerParent.transform.GetChild(i).GetComponent<PTPlayer>();
            orderOfPlayers.Add(playerToAdd);
        }
        
    }

    void Start()
    {
        
        //Fill warPlayers array with Player components
        for (int i = 0; i < PTTableTop.players.Length; ++i)
        {
            warPlayers[i] = PTTableTop.players[i].GetComponent<WarPlayer>();
        }
    }

    protected override bool GameCanEnd()
    {
        foreach(PTPlayer player in orderOfPlayers)
        {
            if(player.GetComponent<WarPlayer>().numPoints >= 3)
            {
                someoneHasWon = true;
            }
        }
        return someoneHasWon || canEnd;
    }

    protected override bool GameCanStart()
    {
        return canStart;
    }

    protected override IEnumerator GameStart()
    {
        canStart = false;
        DeterminePlayerOrder();
        yield return new WaitForSeconds(1);
    }

    protected override IEnumerator GameEnd()
    {
        canEnd = false;
        yield return new WaitForSeconds(0.5f);
    }

    protected override bool RoundCanEnd()
    {
        return numberOfPasses >= orderOfPlayers.Count || canRestart || canEnd;
    }

    protected override bool RoundCanStart()
    {
        return true;
    }

    protected override IEnumerator RoundEnd()
    {
        if (numberOfPasses >= orderOfPlayers.Count)
        {
            canRestart = true;
            yield return null;
        }
        else
        {
            yield return null;
        }
    }

    protected override IEnumerator RoundStart()
    {
        print("Round Starting");
        numberOfPasses = 0;
        yield return new WaitForSeconds(1);
    }

    protected override bool TurnCanEnd(PTPlayer player)
    {
        return player.GetComponent<WarPlayer>().hasFlipped;
    }

    protected override bool TurnCanStart(PTPlayer player)
    {
        return true;
    }

    protected override IEnumerator TurnEnd(PTPlayer player)
    {
        player.GetComponent<WarPlayer>().hasFlipped = false;
        yield return null;
    }

    protected override IEnumerator TurnStart(PTPlayer player)
    {
        currentPlayer = player;
        Debug.Log("It's" + player.name + "'s turn");
        yield return null;
    }
   
    /// <summary>
    /// Determine the player Order
    /// </summary>
    void DeterminePlayerOrder()
    {
        PTPlayer[] playerArray = new PTPlayer[PTTableTop.players.Length];
        PTTableTop.players.CopyTo(playerArray, 0);

        //Sort by player by players active in scene
        System.Array.Sort(playerArray, (a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));

        int rand = UnityEngine.Random.Range(0, playerArray.Length);

        for (int i = rand; i < rand + playerArray.Length; ++i)
        {
            orderOfPlayers.Add(playerArray[i % playerArray.Length]);
        }

    }



}
