using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PlayTable.Unity
{
    public abstract class PTTableTop_TurnBased : PTTableTop
    {
        public PTMono_CanvasRound canvasRound;

        protected override IEnumerator Game()
        {
            //ex. Deal cards to all players
            yield return Game_OnStart();

            while (!Game_IsEnd())
            {
                yield return Round();
            }
            yield return Game_OnEnd();
        }
        protected abstract IEnumerator Game_OnStart();
        protected abstract IEnumerator Game_OnEnd();

        private IEnumerator Round()
        {
            //Show game canvas
            canvasRound.gameObject.SetActive(true);

            yield return Round_OnStart();
            //give the players turn auth one by one
            while (!Round_IsEnd())
            {
                yield return Turn();
            }

            //Show all hands on tabletop, Calcaulate scores in the meanwhile
            canvasRound.gameObject.SetActive(false);
            yield return Round_OnEnd();
        }
        protected abstract IEnumerator Round_OnStart();
        protected abstract IEnumerator Round_OnEnd();
        protected abstract bool Round_IsEnd();

        private IEnumerator Turn()
        {
            yield return Turn_OnStart();

            //Sequence of play
            int next = NextTurnFirstMovePlayer();
            int countPlayed = 0;

            //give the players turn auth one by one
            while (!isTurnEnd())
            {
                //Move
                int curr = (next + countPlayed) % players.Length;
                currPlayer = players[curr];
                yield return Move(currPlayer);
                countPlayed++;
            }
            yield return On_TurnEnd();
        }
        protected abstract IEnumerator Turn_OnStart();
        protected abstract IEnumerator On_TurnEnd();
        protected abstract bool isTurnEnd();
        protected abstract int NextTurnFirstMovePlayer();

        internal IEnumerator Move(PTMono_PlayerTT player)
        {
            yield return Move_OnStart(player);
            yield return new WaitUntil(() => Move_IsEnd(player));
            yield return Move_OnEnd(player);
        }
        protected abstract IEnumerator Move_OnStart(PTMono_PlayerTT player);
        protected abstract IEnumerator Move_OnEnd(PTMono_PlayerTT player);
        protected abstract bool Move_IsEnd(PTMono_PlayerTT player);

        protected override void Awake()
        {
            base.Awake();
            PTMono_CanvasRound existingCanvasRound = FindObjectOfType<PTMono_CanvasRound>();
            if (!existingCanvasRound)
            {
                existingCanvasRound = Instantiate(canvasRound.gameObject).GetComponent<PTMono_CanvasRound>();
                canvasRound = existingCanvasRound;
            }
        }
    }
}