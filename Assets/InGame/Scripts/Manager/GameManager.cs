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
    [Header("Singletone")]
    private static GameManager instance = null;

    [Header("Game State")]
    public bool isClear;
    public bool isFail;

    [Header("Scene Load")]
    public Vector2 savePoint;
    private string selected;

    [Header("Scripts")]
    public MainCamera mainCamera;
    public UIManager uiManager;
    public NetworkManager networkManager;
    public ObjectManager objectManager;
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

        // Scripts
        StartCoroutine(ScriptsCheck());

        // Screen
        Screen.SetResolution(1280, 720, false);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Debug.Log(savePoint + " " + selected);
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
        if (!objectManager) objectManager = gameObject.AddComponent<ObjectManager>();
        if (!astarManager) astarManager = gameObject.AddComponent<AStarManager>();

        if (!mainCamera || !uiManager || !networkManager || !objectManager || !astarManager) {
            yield return null;
            StartCoroutine(ScriptsCheck());
        }
    }

    public void GameStart()
    {
        Time.timeScale = 1;

        StartCoroutine(ScriptsCheck());
        if (selected == default) selected = this.uiManager.GameStart();
        else this.uiManager.GameStart();
        this.objectManager.SpawnPlayer(selected, savePoint);
        this.mainCamera.StartSet();
    }

    public void GameExit()
    {
        // Time.timeScale = 0;

        PhotonView PV = this.GetComponent<PhotonView>();
        if (!PV.IsMine) PV.RequestOwnership();
        PV.RPC("GameLoad", RpcTarget.All);
    }
    [PunRPC]
    private void GameLoad()
    {
        SceneManager.LoadScene(0);
    }
}
