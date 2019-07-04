using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayTable;

public class WarCard : MonoBehaviour {

    PTZone boardZone;

    public int value;
    public Suit suit;

    protected void Awake()
    {

        //GetComponent<PTLocalInput>().OnDropped += (PTTouchFollower touch) =>
        //{
        //    if (touch.touch.hits.ContainsKey(boardZone.GetComponent<Collider>()))
        //    {
        //        print("i shoul;d drop");
        //    }
        //};

    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetValue(int valuePassed)
    {
        value = valuePassed;
    }

}

