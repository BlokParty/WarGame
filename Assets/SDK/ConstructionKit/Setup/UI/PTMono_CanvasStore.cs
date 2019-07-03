using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayTable.Unity;
using System;

public class PTMono_CanvasStore : MonoBehaviour {
    public Button buttonReady;

    private void Awake()
    {
        buttonReady.onClick.AddListener(Ready);
        PTManager.OnConnected += Handler_OnConnected;

    }

    private void Handler_OnConnected(PTMessage msg)
    {
        ResetReadyButton(true);
    }

    private void Start()
    {
        ResetReadyButton(true);
    }

    void Ready()
    {
        PTManager.ReportReady();
        ResetReadyButton(false);
    }

    void ResetReadyButton(bool interactable)
    {
        if (interactable)
        {
            buttonReady.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 50);
            buttonReady.GetComponentInChildren<Text>().text = "Ready";
            buttonReady.GetComponentInChildren<Text>().color = Color.white;
            buttonReady.interactable = true;
        }
        else
        {
            buttonReady.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(350, 50);
            buttonReady.GetComponentInChildren<Text>().text = "Waiting for other players...";
            buttonReady.GetComponentInChildren<Text>().color = new Color(150f / 255f, 150f / 255f, 150f / 255f, 255f / 255f);
            buttonReady.interactable = false;
        }
    }

}
