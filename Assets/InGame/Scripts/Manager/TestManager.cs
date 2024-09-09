using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestManager : MonoBehaviourPunCallbacks
{
    public MainCamera mainCamera;
    public GameObject AccessPanel;
    public Text ConnectText;
    public string selected;

    public void SetCharacter(string player)
    {
        ConnectText.text = selected = player;
    }

    public void GameStart()
    {
        AccessPanel.SetActive(false);
        Connect();
    }

    public void Connect()
    {
        if (!PhotonNetwork.IsConnected) PhotonNetwork.ConnectUsingSettings();
        else Debug.LogWarning("* NetworkManager: Already Connected or Connecting!");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("-> NetworkManager: OnConnectedToMaster");
        PhotonNetwork.JoinOrCreateRoom("Room", new RoomOptions { MaxPlayers = 2 }, null);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("-> NetworkManager: OnJoinedRoom + " + selected);

        if (selected == "Girl") {
            PhotonNetwork.Instantiate("Girl", new Vector3(1, -0.5f, 0), Quaternion.identity);
        }
        else if (selected == "Robot") {
            PhotonNetwork.Instantiate("Robot", new Vector3(-1, -0.5f, 0), Quaternion.identity);
        }
        mainCamera.StartSet();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("-> NetworkManager: OnDisconnected " + cause);
    }
}
