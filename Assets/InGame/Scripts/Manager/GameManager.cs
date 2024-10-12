using Photon.Pun;
using System.Collections;
using Unity.Properties;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Singletone")]
    private static GameManager instance = null;

    [Header("Scene Load")]
    public int saveNumber = -1;
    public Vector2 savePoint;
    private string selected;
    private bool isSceneLoading;

    [Header("Scripts")]
    public MainCamera mainCamera;
    public UIManager uiManager;
    public NetworkManager networkManager;
    public SpawnManager spawnManager;
    public AStarManager astarManager;

    private void Awake()
    {
        // SingleTon
        instance = this;

        // Scene Load
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Screen
        Screen.SetResolution(1280, 720, false);
        // Screen.SetResolution(640, 360, true);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom) {
            StartGame();
        }
    }

    private void OnDestroy()
    {
        // 씬이 전환되거나 오브젝트가 파괴될 때 이벤트 해제
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public static GameManager Instance
    {
        get {
            if (instance == null) return null;
            return instance;
        }
    }

    public void StartGame()
    {
        Time.timeScale = 1;

        StartCoroutine(InitializeGame());
    }

    private IEnumerator InitializeGame()
    {
        yield return StartCoroutine(ValidateScripts());
        yield return StartCoroutine(LoadPlayerData());

        spawnManager.SpawnPlayer(selected, savePoint);
        mainCamera.StartSet();

        yield return StartCoroutine(CheckPlayerCountRoutine());
        StartCoroutine(uiManager.FadeOutCoroutine(null, 2.5f));

        SoundManager.instance.PlayBgm(true);
    }

    private IEnumerator ValidateScripts()
    {
        mainCamera = mainCamera != null ? mainCamera : FindObjectOfType<MainCamera>();
        uiManager = uiManager != null ? uiManager : FindObjectOfType<UIManager>();
        networkManager = networkManager != null ? networkManager : GetComponent<NetworkManager>();
        spawnManager = GetComponent<SpawnManager>() ?? gameObject.AddComponent<SpawnManager>();
        astarManager = GetComponent<AStarManager>() ?? gameObject.AddComponent<AStarManager>();

        yield return null;
    }

    private IEnumerator LoadPlayerData()
    {
        if (PlayerPrefs.HasKey("SavePoint.x")) {
            savePoint.x = PlayerPrefs.GetFloat("SavePoint.x");
            savePoint.y = PlayerPrefs.GetFloat("SavePoint.y");
        }

        selected = PlayerPrefs.HasKey("Selected") ? PlayerPrefs.GetString("Selected") : "";
        if (string.IsNullOrEmpty(selected)) {
            selected = uiManager.StartGame();
        }

        yield return null;
    }

    private IEnumerator CheckPlayerCountRoutine()
    {
        yield return new WaitForSeconds(1.5f);

        if (FindObjectsOfType<Player>().Length < 2) {
            HandleGameFailure();
        }
    }

    public void HandleGameFailure()
    {
        // Time.timeScale = 0;
        if (isSceneLoading) return;

        SoundManager.instance.PlaySfx(SoundManager.Sfx.Lose);

        GetComponent<PhotonView>().RPC(nameof(GameLoad), RpcTarget.All);
    }

    [PunRPC]
    private void GameLoad()
    {
        if (isSceneLoading) return;
        isSceneLoading = true;

        SavePlayerData();
        StartCoroutine(LoadCurSceneRoutine());
    }
    private void SavePlayerData()
    {
        PlayerPrefs.SetFloat("SavePoint.x", savePoint.x);
        PlayerPrefs.SetFloat("SavePoint.y", savePoint.y);
        PlayerPrefs.SetString("Selected", selected);
        PlayerPrefs.Save();
    }

    private IEnumerator LoadCurSceneRoutine()
    {
        yield return StartCoroutine(this.uiManager.FadeInCoroutine(null, 1.25f));

        Player[] players = FindObjectsOfType<Player>();
        if (players.Length > 0) {
            foreach (Player player in players) {
                PhotonView view = player.GetComponent<PhotonView>();
                if (view && view.IsMine && PhotonView.Find(view.ViewID)) {
                    PhotonNetwork.Destroy(view);
                }
            }
        }

        yield return new WaitForSeconds(0.25f);

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void HandleGameClear()
    {
        SoundManager.instance.PlaySfx(SoundManager.Sfx.Win);
        StartCoroutine(ShowClearUIAndPause());
    }

    private IEnumerator ShowClearUIAndPause()
    {
        uiManager.ShowGameClearUI();
        yield return new WaitForSecondsRealtime(3.0f);
        Time.timeScale = 0;
    }


    public void QuitGame()
    {
        // Unity 에디터에서 실행 중인지 확인
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;  // 에디터에서 게임을 멈추기
        #else
            Application.Quit();  // 빌드된 게임 종료
        #endif
    }
}
