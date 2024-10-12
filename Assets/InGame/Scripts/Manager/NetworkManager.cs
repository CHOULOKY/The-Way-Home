using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using System.Collections;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    private void Awake()
    {
        InitializePhotonNetwork();
    }

    private void InitializePhotonNetwork()
    {
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;
    }

    public void Connect()
    {
        if (!PhotonNetwork.IsConnected) PhotonNetwork.ConnectUsingSettings();
        else Debug.LogWarning("* NetworkManager: Already Connected or Connecting!");
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinOrCreateRoom("Room", new RoomOptions { MaxPlayers = 2 }, null);
    }

    public override void OnJoinedRoom()
    {
        ClearPlayerPrefs();

        GameManager.Instance.StartGame();
    }
    private void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteKey("SavePoint.x");
        PlayerPrefs.DeleteKey("SavePoint.y");
        PlayerPrefs.DeleteKey("Selected");
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.PlayerCount < 2) {
            StartCoroutine(LeftRoomRoutine());
        }
    }
    private IEnumerator LeftRoomRoutine()
    {
        yield return StartCoroutine(GameManager.Instance.uiManager.FadeInCoroutine(uiElement: null, duration: 1.0f));
        SceneManager.LoadScene("LobbyScene");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        GameManager.Instance.QuitGame();
    }
}
