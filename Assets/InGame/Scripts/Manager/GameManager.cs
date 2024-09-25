using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Singletone")]
    private static GameManager instance = null;

    [Header("Scene Load")]
    public int saveNumber;
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
            GameStart();
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

    private IEnumerator ScriptsCheck()
    {
        if (!mainCamera) mainCamera = FindObjectOfType<MainCamera>();
        if (!uiManager) uiManager = FindObjectOfType<UIManager>();

        if (!networkManager) networkManager = gameObject.GetComponent<NetworkManager>();
        if (!spawnManager) spawnManager = gameObject.AddComponent<SpawnManager>();
        if (!astarManager) astarManager = gameObject.AddComponent<AStarManager>();

        if (!mainCamera || !uiManager || !networkManager || !spawnManager || !astarManager) {
            yield return null;
            StartCoroutine(ScriptsCheck());
        }
    }

    public void GameStart()
    {
        Time.timeScale = 1;

        StartCoroutine(ScriptsCheck());
        if (PlayerPrefs.HasKey("SavePoint.x")) {
            savePoint.x = PlayerPrefs.GetFloat("SavePoint.x");
            savePoint.y = PlayerPrefs.GetFloat("SavePoint.y");
        }
        if (PlayerPrefs.HasKey("Selected")) {
            selected = PlayerPrefs.GetString("Selected");
            string name = this.uiManager.GameStart();
            if (selected == "") selected = name;
        }
        else {
            selected = this.uiManager.GameStart();
        }
        this.spawnManager.SpawnPlayer(selected, savePoint);
        this.mainCamera.StartSet();
    }

    [PunRPC]
    public void GameFail()
    {
        // Time.timeScale = 0;
        if (isSceneLoading) return;

        if (PhotonNetwork.IsMasterClient)
            this.GetComponent<PhotonView>().RPC("GameLoad", RpcTarget.All);
        else
            this.GetComponent<PhotonView>().RPC("GameFail", RpcTarget.MasterClient);
    }
    [PunRPC]
    private void GameLoad()
    {
        if (isSceneLoading) return;
        isSceneLoading = true;

        PlayerPrefs.SetFloat("SavePoint.x", savePoint.x);
        PlayerPrefs.SetFloat("SavePoint.y", savePoint.y);
        PlayerPrefs.SetString("Selected", selected);
        PlayerPrefs.Save(); // 변경 사항 저장

        if (PhotonNetwork.IsMasterClient) {
            Player[] players = FindObjectsOfType<Player>();
            if (players.Length > 0)
                foreach (Player player in players) {
                    PhotonNetwork.Destroy(player.gameObject);
                }
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GameClear()
    {
        StartCoroutine(ShowClearUIAndPause());
    }

    private IEnumerator ShowClearUIAndPause()
    {
        uiManager.GameClear();
        yield return new WaitForSecondsRealtime(3.0f);
        Time.timeScale = 0;
    }


    public void GameQuit()
    {
        // Unity 에디터에서 실행 중인지 확인
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;  // 에디터에서 게임을 멈추기
        #else
            Application.Quit();  // 빌드된 게임 종료
        #endif
    }
}
