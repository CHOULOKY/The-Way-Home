using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Start")]
    public GameObject AccessPanel;
    public Text ConnectText;
    public string selected;

    public void SetCharacter(string player)
    {
        ConnectText.text = selected = player;
    }

    public string GameStart()
    {
        AccessPanel.SetActive(false);
        return selected;
    }
}
