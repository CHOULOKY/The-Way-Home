using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{

    private static NetworkManager instance;

    private void Awake()
    {
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else if (instance != this)
        {
            Destroy(this.gameObject);
        }
    }

    void Start()
    {
        if (!GameManager.Instance.hasSelectedCharacterInLobby)
        {
            Debug.Log("selectedCharacter property not found");
            PhotonNetwork.Disconnect();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && PhotonNetwork.IsConnected)
            PhotonNetwork.Disconnect();

        if (Input.GetKeyDown(KeyCode.Tab))
            Debug.Log(PhotonNetwork.NetworkClientState.ToString());
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
        Debug.Log("-> NetworkManager: OnJoinedRoom");
        GameManager.Instance.uiManager.GameStart();
        GameManager.Instance.objectManager.SpawnPlayer();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("-> NetworkManager: OnDisconnected " + cause);
        GameManager.Instance.uiManager.GameRoom();
    }
}

// Sample code
/*
public class NetworkManager : MonoBehaviourPunCallbacks
{
    public Text StatusText;
    public InputField roomInput, NickNameInput;

    void Awake() => Screen.SetResolution(640, 360, false);

    // Current network state
    void Update() => StatusText.text = PhotonNetwork.NetworkClientState.ToString();


    public void Connect() => PhotonNetwork.ConnectUsingSettings();
    
    public override void OnConnectedToMaster()
    {
        print("���� ���� �Ϸ�");
        PhotonNetwork.LocalPlayer.NickName = NickNameInput.text;
    }


    public void Disconnect() => PhotonNetwork.Disconnect();

    public override void OnDisconnected(DisconnectCause cause) => print("���� ����");


    public void JoinLobby() => PhotonNetwork.JoinLobby();

    public override void OnJoinedLobby() => print("�κ� ���� �Ϸ�");


    public void CreateRoom() => PhotonNetwork.CreateRoom(roomInput.text, new RoomOptions { MaxPlayers = 2 });

    public void JoinRoom() => PhotonNetwork.JoinRoom(roomInput.text);

    public void JoinOrCreateRoom() => PhotonNetwork.JoinOrCreateRoom(roomInput.text, new RoomOptions { MaxPlayers = 2 }, null);

    public void JoinRandomRoom() => PhotonNetwork.JoinRandomRoom();

    public void LeaveRoom() => PhotonNetwork.LeaveRoom();

    public override void OnCreatedRoom() => print("�� ����� �Ϸ�");

    public override void OnJoinedRoom() => print("�� ���� �Ϸ�");

    public override void OnCreateRoomFailed(short returnCode, string message) => print("�� ����� ����");

    public override void OnJoinRoomFailed(short returnCode, string message) => print("�� ���� ����");

    public override void OnJoinRandomFailed(short returnCode, string message) => print("�� ���� ���� ����");



    [ContextMenu("����")]
    void Info()
    {
        if (PhotonNetwork.InRoom) {
            print("���� �� �̸� : " + PhotonNetwork.CurrentRoom.Name);
            print("���� �� �ο��� : " + PhotonNetwork.CurrentRoom.PlayerCount);
            print("���� �� �ִ��ο��� : " + PhotonNetwork.CurrentRoom.MaxPlayers);

            string playerStr = "�濡 �ִ� �÷��̾� ��� : ";
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++) playerStr += PhotonNetwork.PlayerList[i].NickName + ", ";
            print(playerStr);
        }
        else {
            print("������ �ο� �� : " + PhotonNetwork.CountOfPlayers);
            print("�� ���� : " + PhotonNetwork.CountOfRooms);
            print("��� �濡 �ִ� �ο� �� : " + PhotonNetwork.CountOfPlayersInRooms);
            print("�κ� �ִ���? : " + PhotonNetwork.InLobby);
            print("����ƴ���? : " + PhotonNetwork.IsConnected);
        }
    }
}
*/
