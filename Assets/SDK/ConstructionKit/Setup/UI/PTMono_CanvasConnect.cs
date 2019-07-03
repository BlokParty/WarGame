using System.Collections;
using UnityEngine;
using PlayTable.Unity;
using System;
using UnityEngine.UI;

public class PTMono_CanvasConnect : MonoBehaviour {
    public Transform scrollviewContent;
    public PTMono_NameCard cardTemplate;
    public Button playButton;
    public InputField inputPlayerName;
    const float UpdateContentTimer = 1;

    private void Awake()
    {
        //OnConnected Delegate
        PTManager.OnConnected += Handler_OnConnected;

        //playButton OnClick Delegate
        if (playButton)
            playButton.onClick.AddListener(OnPlayButtonClicked);

        //playername input onValueChanged delegate
        if (inputPlayerName)
        {
            inputPlayerName.onValueChanged.AddListener((string value) =>
            {
                PTManager.playerName = value != "" ? value : PTManager.playerName;
            });
        }

    }

    private void OnPlayButtonClicked()
    {
        gameObject.SetActive(false);
        PTManager.ReportReady();
    }

    private void Start()
    {
        HidePlayButton();
        StartCoroutine(UpdateContent());


    }

    private void Handler_OnConnected(PTMessage msg)
    {
        if (!PTManager.isTableTop)
        {
            gameObject.SetActive(false);
        }
    }

    public IEnumerator UpdateContent()
    {
        while (true)
        {
            //playername input default placeholder
            if (inputPlayerName)
            {
                inputPlayerName.placeholder.GetComponent<Text>().text = PTManager.playerName;
            }

            foreach (Transform child in scrollviewContent)
            {
                Destroy(child.gameObject);
            }

            if (PTManager.isTableTop)
            {
                bool allReady = true;
                foreach (PTPlayer player in PTManager.Players)
                {
                    GameObject newCard = Instantiate(cardTemplate.gameObject, scrollviewContent);
                    newCard.GetComponent<PTMono_NameCard>().UpdateContent(player);
                    if (!PTManager.isReady(player))
                    {
                        allReady = false;
                    }
                }
                if (PTManager.Players.Count > 0
                    && PTManager.Players.Count >= PTManager.minimumPlayer
                    && PTManager.Players.Count <= PTManager.maximumPlayer
                    && allReady)
                {
                    ShowPlayButton();
                }
            }
            else
            {
                foreach (PTSession session in PTManager.NearbySessions)
                {
                    GameObject newCard = Instantiate(cardTemplate.gameObject, scrollviewContent);
                    newCard.GetComponent<PTMono_NameCard>().UpdateContent(session);
                }
                HidePlayButton();
            }

            yield return new WaitForSeconds(UpdateContentTimer);
        }
    }
    public void HidePlayButton()
    {
        if (playButton)
        {
            playButton.gameObject.SetActive(false);
        }
    }
    public void ShowPlayButton()
    {
        if (playButton)
        {
            playButton.gameObject.SetActive(true);
        }
    }
}
