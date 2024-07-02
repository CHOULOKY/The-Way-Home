using System.Collections;
using System.Collections.Generic;
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
        AccessPanel.SetActive(true);
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

        GameManager.Instance.GameStart();
    }

    public void GamePause()
    {

    }
}
