using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Runtime.CompilerServices;
public class LobbyManager : MonoBehaviourPunCallbacks, IInRoomCallbacks
{
    [Header("DisconnectPanel")]
    public TMP_InputField NickNameInput;
    public TMP_Text WarningText;

    [Header("LobbyPanel")]
    public GameObject LobbyPanel;
    public TMP_Text WelcomeText;
    public TMP_Text LobbyInfoText;
    public Button[] CellBtn;
    public Button PreviousBtn;
    public Button NextBtn;
    private string roomName;

    [Header("RoomPanel")]
    private bool isHost;
    public GameObject RoomPanel;
    public TMP_Text ListText;
    public TMP_Text RoomInfoText;
    public Transform ChatContent; // ScrollView의 Content
    public ScrollRect ChatScrollRect; // ScrollView
    public TMP_InputField ChatInput;

    [Header("SelectChapter")]
    public ChapterDatabase chapterDB;
    public Text chapterText;
    public Button ChatperNext;
    public Button ChatperPrevious;
    public SpriteRenderer artworkSprite;
    private int selectedOption = 0;

    [Header("GameButton")]
    public Button GameBtn;
    private bool isGuestReady = false;

    [Header("ETC")]
    public PhotonView PV;

    List<RoomInfo> myList = new List<RoomInfo>();
    int currentPage = 1, maxPage, multiple;


    #region Room list
    // ◀버튼 -2 , ▶버튼 -1 , 셀 숫자
    public void MyListClick(int num)
    {
        if (num == -2) --currentPage;
        else if (num == -1) ++currentPage;
        // num이 0 이상인 경우: myList 배열에서 해당 인덱스의 방에 참가
        else PhotonNetwork.JoinRoom(myList[multiple + num].Name);
        MyListRenewal();
    }
    void MyListRenewal()
    {
        // 최대페이지
        maxPage = (myList.Count % CellBtn.Length == 0) ? myList.Count / CellBtn.Length : myList.Count / CellBtn.Length + 1;
        // 이전, 다음버튼
        PreviousBtn.interactable = (currentPage <= 1) ? false : true;
        NextBtn.interactable = (currentPage >= maxPage) ? false : true;
        // 페이지에 맞는 리스트 대입
        multiple = (currentPage - 1) * CellBtn.Length;
        for (int i = 0; i < CellBtn.Length; i++)
        {
            CellBtn[i].interactable = (multiple + i < myList.Count) ? true : false;
            CellBtn[i].transform.GetChild(0).GetComponent<TMP_Text>().text = (multiple + i < myList.Count) ? myList[multiple + i].Name : "";
            CellBtn[i].transform.GetChild(1).GetComponent<TMP_Text>().text = (multiple + i < myList.Count) ? myList[multiple + i].PlayerCount + "/" + myList[multiple + i].MaxPlayers : "";
        }
    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        int roomCount = roomList.Count;
        for (int i = 0; i < roomCount; i++)
        {
            if (!roomList[i].RemovedFromList)
            {
                // 방이 삭제되지 않은 경우
                // 해당 방이 없으면 방을 추가
                if (!myList.Contains(roomList[i])) myList.Add(roomList[i]);
                // 이미 해당 방이 있으면 기존 방 정보를 업데이트
                else myList[myList.IndexOf(roomList[i])] = roomList[i];
            }
            // 해당 방이 있는지 확인하고 존재하면 해당 방을 삭제
            else if (myList.IndexOf(roomList[i]) != -1) myList.RemoveAt(myList.IndexOf(roomList[i]));
        }
        MyListRenewal();
    }
    #endregion

    #region Server connection
    void Awake() => Screen.SetResolution(1280, 720, false);
    // void Awake() => Screen.SetResolution(640, 360, true);
    void Update()
    {
        if (LobbyPanel.activeSelf)
            // 로비에 몇명있는지, 접속한 인원수
            LobbyInfoText.text = "로비:" + (PhotonNetwork.CountOfPlayers - PhotonNetwork.CountOfPlayersInRooms) + "/ 접속:" + PhotonNetwork.CountOfPlayers;
    }

    // Photon 서버에 연결
    public void Connect()
    {
        if (string.IsNullOrEmpty(NickNameInput.text))
        {
            WarningText.text = "닉네임을 입력해주세요!";
            return;
        }
        WarningText.text = "";
        PhotonNetwork.LocalPlayer.NickName = NickNameInput.text;
        PhotonNetwork.ConnectUsingSettings();
    }

    // 마스터 서버에 연결되면 로비 참가
    public override void OnConnectedToMaster() => PhotonNetwork.JoinLobby();
    public override void OnJoinedLobby()
    {
        LobbyPanel.SetActive(true);
        RoomPanel.SetActive(false);

        WelcomeText.text = PhotonNetwork.LocalPlayer.NickName + "님 환영합니다";
        myList.Clear();
    }

    // 로비 패널에서 x버튼 누르면 서버 연결 끊김
    public void Disconnect() => PhotonNetwork.Disconnect();
    public override void OnDisconnected(DisconnectCause cause)
    {
        LobbyPanel.SetActive(false);
        RoomPanel.SetActive(false);
    }
    #endregion

    #region Room
    // 호스트 이름대로 최대 인원 2명인 방 생성
    // 방 생성 후 OnJoinedRoom() 콜백
    public void CreateRoom()
    {
        string hostName = PhotonNetwork.LocalPlayer.NickName;
        roomName = hostName + "의 방";
        PhotonNetwork.CreateRoom(roomName, new RoomOptions { MaxPlayers = 2 });
    }
    public void JoinRandomRoom() => PhotonNetwork.JoinRandomRoom();
    public void LeaveRoom() => PhotonNetwork.LeaveRoom();
    public override void OnJoinedRoom()
    {
        // 룸 패널 활성화
        RoomPanel.SetActive(true);
        // 방 정보 갱신
        RoomRenewal();
        // 채팅 초기화
        foreach (Transform child in ChatContent)
        {
            Destroy(child.gameObject);
        }

        GameBtn.onClick.RemoveAllListeners();

        isHost = PhotonNetwork.IsMasterClient;
        ChatperNext.interactable = isHost;
        ChatperPrevious.interactable = isHost;

        if (isHost)
        {
            GameBtn.interactable = false;
            GameBtn.GetComponentInChildren<TMP_Text>().text = "Start Game";
            GameBtn.onClick.AddListener(OnHostGameStart);
        }
        else
        {
            GameBtn.interactable = false;
            GameBtn.GetComponentInChildren<TMP_Text>().text = "Ready";
            GameBtn.onClick.AddListener(OnGuestReady);
        }

        #region Scene Load
        PlayerPrefs.DeleteKey("SavePoint.x");
        PlayerPrefs.DeleteKey("SavePoint.y");
        PlayerPrefs.DeleteKey("Selected");
        #endregion
    }
    public override void OnCreateRoomFailed(short returnCode, string message) { roomName = ""; CreateRoom(); }
    public override void OnJoinRandomFailed(short returnCode, string message) { roomName = ""; CreateRoom(); }
    // 플레이어가 방에 들어왔을때 모든 플레이어에게 호출됨
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        RoomRenewal();
        ChatRPC("<color=yellow>" + newPlayer.NickName + "님이 참가하셨습니다</color>");
    }
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        RoomRenewal();
        ChatRPC("<color=yellow>" + otherPlayer.NickName + "님이 퇴장하셨습니다</color>");

        // CharacterSelection 클래스 인스턴스를 찾아와서 나간 플레이어의 선택 해제 처리
        FindObjectOfType<CharacterSelection>().HandlePlayerLeftRoom(otherPlayer);

        if (!PhotonNetwork.IsMasterClient)
        {
            PV.RPC("GuestReady", RpcTarget.MasterClient, false);
        }
    }
    void RoomRenewal()
    {
        ListText.text = "";
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            ListText.text += PhotonNetwork.PlayerList[i].NickName + ((i + 1 == PhotonNetwork.PlayerList.Length) ? "" : ", ");
        RoomInfoText.text = PhotonNetwork.CurrentRoom.Name + " / 현재 " + PhotonNetwork.CurrentRoom.PlayerCount + " / 최대 " + PhotonNetwork.CurrentRoom.MaxPlayers;
    }
    // 방 호스트가 방을 나갔을 시에 호출
    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        // 새로운 마스터 클라이언트가 로컬 플레이어(자신)인 경우
        if (PhotonNetwork.LocalPlayer == newMasterClient)
        {
            // 방에서 나가기
            PhotonNetwork.LeaveRoom();
            RoomPanel.SetActive(false);
            myList.Clear();
        }
    }
    #endregion

    #region Room settings
    public void NextOption()
    {
        selectedOption++;
        if (selectedOption >= chapterDB.ChapterCount)
        {
            selectedOption = 0;
        }
        photonView.RPC("UpdateChapter", RpcTarget.AllBuffered, selectedOption);
    }
    public void BackOption()
    {
        selectedOption--;
        if (selectedOption < 0)
        {
            selectedOption = chapterDB.ChapterCount - 1;
        }
        photonView.RPC("UpdateChapter", RpcTarget.AllBuffered, selectedOption);
    }
    [PunRPC]
    private void UpdateChapter(int selectedOption)
    {
        Chapter chapter = chapterDB.GetChapter(selectedOption);
        //artworkSprite.sprite = chapter.chapterSprite;
        chapterText.text = chapter.chapterNum;
    }
    #endregion

    #region Chat
    public void Send()
    {
        // 방에있는 모든 플레이어에게 전달
        // RPC(호출하는 함수, 타겟)
        PV.RPC("ChatRPC", RpcTarget.All, PhotonNetwork.NickName + " : " + ChatInput.text);
        ChatInput.text = "";
    }
    [PunRPC] // RPC는 플레이어가 속해있는 방 모든 인원에게 전달한다
    void ChatRPC(string msg)
    {
        // 새 메시지 오브젝트 생성
        GameObject chatMessage = Instantiate(Resources.Load("ChatPrefab") as GameObject, ChatContent);
        TMP_Text chatText = chatMessage.GetComponent<TMP_Text>();
        chatText.text = msg;
        // 스크롤을 맨 아래로 이동
        Canvas.ForceUpdateCanvases();
        ChatScrollRect.verticalNormalizedPosition = 0f;
    }
    #endregion

    #region GameStart Btn
    public void UpdateGameButton()
    {
        CharacterSelection characterSelection = FindObjectOfType<CharacterSelection>();
        bool isCharacterSelected = characterSelection.IsCharacterSelected(PhotonNetwork.LocalPlayer.ActorNumber);

        if (PhotonNetwork.IsMasterClient)
        {
            GameBtn.interactable = isCharacterSelected && isGuestReady;
        }
        else
        {
            GameBtn.interactable = isCharacterSelected;

            isGuestReady = false;
            GameBtn.GetComponentInChildren<TMP_Text>().text = "Ready";
        }
    }

    public void OnGuestReady()
    {
        isGuestReady = !isGuestReady;
        PV.RPC("GuestReady", RpcTarget.MasterClient, isGuestReady);
        GameBtn.GetComponentInChildren<TMP_Text>().text = isGuestReady ? "Cancel" : "Ready";
    }

    [PunRPC]
    public void GuestReady(bool guestReady)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            isGuestReady = guestReady;

            CharacterSelection characterSelection = FindObjectOfType<CharacterSelection>();
            bool isHostCharacterSelected = characterSelection.IsCharacterSelected(PhotonNetwork.LocalPlayer.ActorNumber);

            GameBtn.interactable = isHostCharacterSelected && isGuestReady;
        }
    }

    private void OnHostGameStart()
    {
        if (PhotonNetwork.IsMasterClient && isGuestReady)
        {
            StartCoroutine(StartGameCoroutine("Chapter 1"));
        }
    }
    private IEnumerator StartGameCoroutine(string sceneName)
    {
        ScreenFader screenFader = FindObjectOfType<ScreenFader>();
        PV.RPC("Fade", RpcTarget.All, 1.0f, 1.0f);
        yield return new WaitForSeconds(1.0f);

        PV.RPC("SetRoomPanelActive", RpcTarget.All);
        PV.RPC("LoadGameScene", RpcTarget.Others, sceneName);
        yield return new WaitForSeconds(0.5f);

        PhotonNetwork.LoadLevel(sceneName);
    }

    [PunRPC]
    public void SetRoomPanelActive()
    {
        RoomPanel.SetActive(false);
    }

    [PunRPC]
    private void LoadGameScene(string sceneName)
    {
        PhotonNetwork.LoadLevel(sceneName);
    }
    #endregion
}
