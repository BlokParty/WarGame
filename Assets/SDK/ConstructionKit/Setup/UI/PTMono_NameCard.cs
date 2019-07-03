using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayTable.Unity;

public class PTMono_NameCard : MonoBehaviour {
    public Text textName;
    public Image background;
    public Image profileImage;
    public Text textReady;
    public Button button;

    private void Start()
    {
    }

    public void UpdateContent(PTSession session)
    {
        textName.text = session.senderName + "'s\n" + session.gameName;
        textReady.text = "";
        background.enabled = Random.Range(0, 2) == 0 ? false : true;
        background.color = new Color(Random.Range(0, 1.0f), Random.Range(0, 1.0f), Random.Range(0, 1.0f), 0.8f);

        button.onClick.AddListener(() => {
            PTManager.Connect(session.senderName, session.gameName);
        });
    }
    public void UpdateContent(PTPlayer player)
    {
        textName.text = player.name;
        textReady.text = "";
        background.enabled = Random.Range(0, 2) == 0 ? false : true;
        background.color = new Color(Random.Range(0, 1.0f), Random.Range(0, 1.0f), Random.Range(0, 1.0f), 0.8f);
        button.onClick.AddListener(() => {
            Debug.Log(player.name + " clicked");
        });
    }
}
