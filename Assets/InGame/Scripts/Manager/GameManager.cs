using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using Unity.VisualScripting;
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

    [Header("Scripts")]
    public MainCamera mainCamera;
    public UIManager uiManager;
    public NetworkManager networkManager;
    public SpawnManager spawnManager;
    public AStarManager astarManager;

    private void Awake()
    {
        // SingleTon
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else {
            Destroy(this.gameObject);
            return;
        }

        // Scene Load
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Screen
        Screen.SetResolution(1280, 720, false);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom) {
            GameStart();
        }
    }

    private void Start()
    {
        // Time.timeScale = 0;
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
            this.uiManager.GameStart();
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

        if (PhotonNetwork.IsMasterClient)
            this.GetComponent<PhotonView>().RPC("GameLoad", RpcTarget.All);
        else
            this.GetComponent<PhotonView>().RPC("GameFail", RpcTarget.MasterClient);
    }
    [PunRPC]
    private void GameLoad()
    {
        PlayerPrefs.SetFloat("SavePoint.x", savePoint.x);
        PlayerPrefs.SetFloat("SavePoint.y", savePoint.y);
        PlayerPrefs.SetString("Selected", selected);
        PlayerPrefs.Save(); // 변경 사항 저장

        PhotonView PV = this.GetComponent<PhotonView>();
        if (PV != null && PhotonNetwork.IsMasterClient)
            PhotonNetwork.Destroy(this.GetComponent<PhotonView>());

        SceneManager.LoadScene(0);
    }

    public void GameClear()
    {
        StartCoroutine(ShowClearUIAndPause());
    }

    private IEnumerator ShowClearUIAndPause()
    {
        uiManager.GameClear();
        yield return new WaitForSecondsRealtime(1.0f);
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
