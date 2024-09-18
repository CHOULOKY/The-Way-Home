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
    public void GameExit()
    {
        // Time.timeScale = 0;

        if (PhotonNetwork.IsMasterClient)
            this.GetComponent<PhotonView>().RPC("GameLoad", RpcTarget.All);
        else
            this.GetComponent<PhotonView>().RPC("GameExit", RpcTarget.MasterClient);
    }
    [PunRPC]
    private void GameLoad()
    {
        PlayerPrefs.SetFloat("SavePoint.x", savePoint.x);
        PlayerPrefs.SetFloat("SavePoint.y", savePoint.y);
        PlayerPrefs.SetString("Selected", selected);
        PlayerPrefs.Save(); // 변경 사항 저장

        PhotonView PV = this.GetComponent<PhotonView>();
        if (PV != null)
            if (PhotonNetwork.IsMasterClient)
                PhotonNetwork.Destroy(this.GetComponent<PhotonView>());
            else {
                Destroy(this.gameObject);
                PhotonNetwork.Instantiate("GameManager", Vector2.zero, Quaternion.identity);
            }

        SceneManager.LoadScene(0);
    }
    /*
    [PunRPC]
    private void RequestDestroyPV()
    {
        if (PhotonNetwork.IsMasterClient) {
            PhotonNetwork.Instantiate("GameManager", Vector2.zero, Quaternion.identity);
            PhotonNetwork.Destroy(this.GetComponent<PhotonView>());
        }
    }
    */
}
