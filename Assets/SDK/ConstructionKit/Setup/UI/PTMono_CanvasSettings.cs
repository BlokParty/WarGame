using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using PlayTable.Unity;

public class PTMono_CanvasSettings : MonoBehaviour {
    public InputField inputHostName;
    public InputField inputGameName;

    public Slider slider_minPlayer;
    Text textSliderMinPlayer;
    public Slider slider_maxPlayer;
    Text textSliderMaxPlayer;
    public Slider slider_Bots;
    Text textSliderBots;

    const string nameSliderMinPlayer = "Min Player: ";
    const string nameSliderMaxPlayer = "Max Player: ";
    const string nameSliderBots = "Bots: ";
    const int DEFAULT_MAX_PLAYER = 4;
    const int DEFAULT_BOTS = 0;

    public Dropdown dropdownTeam;
    public Button buttonDone;

    public delegate void IntDelegate(int value);
    public IntDelegate OnDropdownTeamValueChanged;

    private void Awake()
    {
        //Find the rule
        PTTableTop rule = FindObjectOfType<PTTableTop>();

        //bots
        textSliderBots = slider_Bots.transform.Find("Handle Slide Area").Find("Handle").Find("Title").GetComponent<Text>();
        slider_Bots.value = DEFAULT_BOTS;
        slider_Bots.onValueChanged.AddListener((float value) =>
        {
            rule.botCount = (int)value;
            textSliderBots.text = nameSliderBots + (int)value;
        });

        //Min and max players
        textSliderMinPlayer = slider_minPlayer.transform.Find("Handle Slide Area").Find("Handle").Find("Title").GetComponent<Text>();
        textSliderMaxPlayer = slider_maxPlayer.transform.Find("Handle Slide Area").Find("Handle").Find("Title").GetComponent<Text>();
        slider_maxPlayer.value = DEFAULT_MAX_PLAYER;
        slider_minPlayer.onValueChanged.AddListener((float value) =>
        {
            PTManager.minimumPlayer = (int)value;
            textSliderMinPlayer.text = nameSliderMinPlayer + value;
            slider_maxPlayer.minValue = value;
            int maxBots = PTManager.maximumPlayer - PTManager.minimumPlayer;
            slider_Bots.maxValue = maxBots > 0 ? maxBots : 0;
        });
        slider_maxPlayer.onValueChanged.AddListener((float value) =>
        {
            PTManager.maximumPlayer = (int)value;
            textSliderMaxPlayer.text = nameSliderMaxPlayer + value;
            int maxBots = PTManager.maximumPlayer - PTManager.minimumPlayer;
            slider_Bots.maxValue = maxBots > 0 ? maxBots : 0;
        });

        //dropdownTeam
        dropdownTeam.onValueChanged.AddListener((int value) =>
        {
            if (OnDropdownTeamValueChanged != null)
            {
                OnDropdownTeamValueChanged(value);
            }
        });

        //playername input onValueChanged delegate
        PTManager.playerName = SystemInfo.deviceUniqueIdentifier.Substring(0, 10);
        inputHostName.placeholder.GetComponent<Text>().text = PTManager.playerName;

        if (inputHostName)
        {
            inputHostName.onValueChanged.AddListener((string value) =>
            {
                PTManager.playerName = value;// != "" ? value : PTManager.playerName;
            });
        }

        //gamename input onValueChanged delegate
        if (inputGameName)
        {
            inputGameName.onValueChanged.AddListener((string value) =>
            {
                PTManager.gameName = value != "" ? value : PTManager.gameName;
            });
        }

        //ButtonDone
        buttonDone.onClick.AddListener(() => {
            gameObject.SetActive(false);

        });

    }
    private void Start()
    {
        UpdateContent();
    }

    private void Handler_OnNameSliderValueChanged()
    {
        UpdateContent();
    }
    private void UpdateContent()
    {
        //sliders
        textSliderMinPlayer.text = nameSliderMinPlayer + slider_minPlayer.value;
        textSliderMaxPlayer.text = nameSliderMaxPlayer + slider_maxPlayer.value;
        textSliderBots.text = nameSliderBots + slider_Bots.value;

        //inputs
        inputGameName.placeholder.GetComponent<Text>().text = SceneManager.GetActiveScene().name;
    }

    public void SetPlayerNum(bool visible)
    {
        slider_minPlayer.gameObject.SetActive(visible);
        slider_maxPlayer.gameObject.SetActive(visible);

    }
}
