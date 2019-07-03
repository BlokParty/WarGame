using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayTable;
using PlayTable.Util;
using PlayTable.Unity;
using UnityEngine.UI;

public abstract class PTTableTop_Playingcard : PTTableTop_TurnBased
{
    public PTMono_PlayingcardDeck deck;

    public abstract void OnPlayerPlayedCard(PTMono_PlayerTT player, Card card);
    public abstract bool canPlay(PTMono_PlayerTT player, Playingcard card);

}
