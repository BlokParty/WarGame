using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayTable;
using System;

public class PTSeat : MonoBehaviour {
    public SpriteRenderer spriteDragHere;
    public PTZone zone;

    public bool isFull { get { return !spriteDragHere.gameObject.activeSelf; } }
    public PTSeatSelector playerSitting { get { return GetComponentInChildren<PTSeatSelector>(); } }

    private void Awake()
    {
        zone = GetComponent<PTZone>();
    }

    public void LetSit(Transform player)
    {
        if (player)
        {
            zone.Add(player);
            player.transform.SetPositionAndRotation(transform.position, transform.rotation);
        }
        UpdateLabel();
    }

    public void UpdateLabel()
    {
        spriteDragHere.gameObject.SetActive(zone.Count == 0);

    }

    /*
    private void HandlerOnTouchEnd_BeginOnThis(PTTouch touch)
    {
        PTPlayer followingPlayer;
        foreach (PTTouchFollower follower in touch.followers)
        {
            if (follower.collider)
            {
                followingPlayer = follower.collider.GetComponent<PTPlayer>();
            }
        }
        
    }

    private void HandlerOnTouchEnd_EndOnThis(PTTouch touch)
    {
        foreach (PTTouchFollower follower in touch.followers)
        {
            if(follower.collider)
            {
                PTPlayer followingPlayer = follower.collider.GetComponent<PTPlayer>();
                if (followingPlayer)
                {
                    follower.transform.SetPositionAndRotation(transform.position, transform.rotation);
                    spriteDragHere.gameObject.SetActive(true);
                }
                
            }
        }
    }*/
}
