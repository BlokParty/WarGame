using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayTable;

public class PTButtonAddPlayer : MonoBehaviour {
    public Transform spawn;
    public int Count { get; private set; }

    private PTLocalInput localInput;


    private void Awake()
    {
        localInput = GetComponent<PTLocalInput>();
        Count = 0;

        localInput.OnClicked += (PTTouch touch) =>
        {
            PTPlayer prefPlayer = PTTabletopManager.singleton.playerPrefab;
            if (prefPlayer != null)
            {
                //Instantiate new player thumnail
                PTPlayer newPlayer = Instantiate(prefPlayer.gameObject).GetComponent<PTPlayer>();
                newPlayer.transform.position = transform.position;
                newPlayer.transform.SetParent(spawn);
                prefPlayer.blueprints[Count++ % prefPlayer.blueprints.Length].ApplyTo(newPlayer);

                //Add button
                transform.SetAsLastSibling();
                gameObject.SetActive(PTTableTop.players.Length - 1 < PTTableTop.maxPlayer);
                spawn.GetComponent<PTZone>().Arrange();
            }
            
            /*PTPlayer newPlayer = PTTabletopManager.singleton.AddPlayer();
            if (newPlayer)
            {
                newPlayer.transform.position = transform.position;
            }
            transform.SetAsLastSibling();
            gameObject.SetActive(PTTableTop.players.Length < PTTableTop.maxPlayer);
            PTTabletopManager.singleton.spawns.GetComponent<PTZone>().Arrange();
            */
        };
    }
}
