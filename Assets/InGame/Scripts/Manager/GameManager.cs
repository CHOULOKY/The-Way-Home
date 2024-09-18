using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    [Header("----------Game State")]
    public bool isClear;
    public bool isFail;

    [Header("----------Singletone")]
    private static GameManager instance = null;

    [Header("----------Scripts")]
    public NetworkManager networkManager;
    public UIManager uiManager;
    public ObjectManager objectManager;
    public MainCamera mainCamera;
    public AStarManager astarManager;
    public CheckpointManager checkpointManager;
    public RespawnManager respawnManager;

    [Header("----------Select Character In Lobby")]
    public bool hasSelectedCharacterInLobby;

    void Awake()
    {
        // Screen initialization
        Screen.SetResolution(1280, 720, false);

        // SingleTon initialization
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else {
            Destroy(this.gameObject);
        }

        hasSelectedCharacterInLobby = PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("selectedCharacter");
    }

    // Scripts initialization
    private void InitializeManagers()
    {
        if (networkManager == null)
            networkManager = FindObjectOfType<NetworkManager>();

        if (uiManager == null)
            uiManager = FindObjectOfType<UIManager>();

        if (objectManager == null)
            objectManager = FindObjectOfType<ObjectManager>();

        if (mainCamera == null)
            mainCamera = FindObjectOfType<MainCamera>();

        if (checkpointManager == null)
            checkpointManager = FindObjectOfType<CheckpointManager>();

        if (respawnManager == null)
            respawnManager = FindObjectOfType<RespawnManager>();

        if (astarManager == null)
            astarManager = FindObjectOfType<AStarManager>();
    }

    // �ٸ� ��ũ��Ʈ���� �� �ν��Ͻ��� �����ϱ� ���� ������Ƽ
    public static GameManager Instance
    {
        get {
            if (instance == null) return null;
            return instance;
        }
    }

    void Start()
    {
        Time.timeScale = 0;

        // If the client selected a character in the lobby
        if (hasSelectedCharacterInLobby)
        {
            objectManager.ReadyForSpawn();
            GameStart();
        }
    }

    void Update()
    {
        if (isFail) GameFail();
        else if (isClear) GameClear();

        // Test Code
        if (Input.GetKeyDown(KeyCode.Backspace)) ExitGame();

        // Test Code
        if (Input.GetKeyDown(KeyCode.R)) RespawnAtCheckpoint();
    }


    public void GameStart()
    {
        Time.timeScale = 1;

        if (!hasSelectedCharacterInLobby)
        {
            if (GameManager.Instance.uiManager.selected == "")
            {
                Debug.LogWarning("* GameManager: Select a Character!");
                return;
            }
            networkManager.Connect();
            uiManager.GameStart();
        }
        mainCamera.StartSet();
        Debug.LogWarning("* GameManager: GameStart!");
    }

    public void GamePause()
    {
        Time.timeScale = 0;
    }

    public void GameFail()
    {

    }
    IEnumerator FailRoutine()
    {
        Time.timeScale = 0.25f;

        yield return new WaitForSecondsRealtime(5f);

        Time.timeScale = 0;
    }

    public void GameClear()
    {
        Time.timeScale = 1;
        uiManager.GameClear();
    }

    public void RespawnAtCheckpoint()
    {
        respawnManager.RespawnAtCheckpoint();
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(); // ���ø����̼� ����
#endif
    }

    // 포톤뷰 있는 매니저
    public void DestroyManagers()
    {
        if (objectManager != null) Destroy(objectManager.gameObject);
        if (checkpointManager != null) Destroy(checkpointManager.gameObject);
        if (respawnManager != null) Destroy(respawnManager.gameObject);
    }

    public void ReloadScene()
    {
        objectManager.DestroyPlayer();

        DestroyManagers();

        // 현재 씬 리로드
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);

        StartCoroutine(ReloadSceneRoutine());
    }

    private IEnumerator ReloadSceneRoutine()
    {
        yield return new WaitUntil(() => SceneManager.GetActiveScene().isLoaded);

        // 매니저 재지정
        InitializeManagers();
    }
}
