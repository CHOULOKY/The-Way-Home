using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("RoomSet")]
    public string selected;

    [Header("Start")]
    public GameObject AccessPanel;
    public Text ConnectText;

    [Header("Pause")]
    public GameObject PauseButton;

    [Header("Fail")]
    public GameObject FailButton;

    [Header("Clear")]
    public GameObject ClearButton;

    void Start()
    {
        // If the client has selected a character
        if (GameManager.Instance.hasSelectedCharacterInLobby)
        {
            GameStart();
        }
        else
        {
            AccessPanel.SetActive(true);
        }
    }


    public void SetCharacter(string player)
    {
        ConnectText.text = selected = player;
    }

    public void GameRoom()
    {
        AccessPanel.SetActive(true);
    }

    public void GameStart()
    {
        AccessPanel.SetActive(false);
    }

    public void GamePause()
    {

    }
}
