using PlayTable;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class WarPlayer : MonoBehaviour
{

    public int numPoints;
    public bool hasFlipped;

    private void Start()
    {
        
    }

    public void StartGameInit(bool value)
    {
        gameObject.SetActive(value);
    }

 


}
