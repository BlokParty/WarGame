using PlayTable;
using UnityEngine;

public class PTSeatSelector : MonoBehaviour {
    private PTLocalInput localInput;
    public PTSeat seat { get { return GetComponentInParent<PTSeat>(); } }

    private void Awake()
    {
        localInput = GetComponent<PTLocalInput>();

        localInput.OnDragBegin += HandlerOnDragBegin;
        localInput.OnDropped += HandlerOnDropped;
    }

    private void HandlerOnDragBegin(PTTouch touch)
    {
        PTSeat lastSeat = seat;
        transform.SetParent(null);
        if (lastSeat)
        {
            lastSeat.UpdateLabel();
        }
    }

    private void HandlerOnDropped(PTTouchFollower follower)
    {
        Debug.Log("HandlerOnDropped");

        PTSeat targetSeat = null;
        foreach (Collider collider in follower.touch.hits.Keys)
        {
            if (collider.GetComponent<PTSeat>())
            {
                targetSeat = collider.GetComponent<PTSeat>();
                break;
            }
        }

        if (targetSeat != null)
        {
            //on seat
            if (targetSeat != seat)
            {
                if (seat != null)
                {
                    //exchange seat
                    seat.LetSit(targetSeat.playerSitting.transform);
                }
                else
                {
                    //move the sitting player to pool
                    PTTabletopManager.singleton.spawns.GetComponent<PTZone>().Add(targetSeat.playerSitting);
                }
                targetSeat.LetSit(transform);

            }
        }
        else
        {
            //seat off
            PTTabletopManager.singleton.spawns.GetComponent<PTZone>().GetComponent<PTZone>().Add(this);
        }
        PTTabletopManager.singleton.spawns.GetComponent<PTZone>().GetComponent<PTZone>().Arrange();

        /*

        //Find the hovering seat
        PTPlayerSeat hoverSeat = null;
        foreach (Collider collider in follower.touch.hits.Keys)
        {
            if (collider.GetComponent<PTPlayerSeat>())
            {
                hoverSeat = collider.GetComponent<PTPlayerSeat>();
                break;
            }
        }

        if (hoverSeat != null)
        {
            //seat on
            if (hoverSeat.isFull && player != hoverSeat.player)
            {
                hoverSeat.Reset();
            }
            hoverSeat.Seat(player);

            if (seat && !seat.isFull)
            {
                seat.Reset();
            }
            seat = hoverSeat;
        }
        else
        {
            //seat off
            if (seat)
            {
                seat.Reset();
                seat = null;
            }
        }
        PTTabletopManager.singleton.spawns.GetComponent<PTZone>().Arrange();
        */
    }
}
