using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using UnityEngine;

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

        // Scripts initialization
        networkManager = FindAnyObjectByType<NetworkManager>();
        uiManager = FindAnyObjectByType<UIManager>();
        objectManager = FindAnyObjectByType<ObjectManager>();
        mainCamera = FindObjectOfType<MainCamera>();
    }
    // 다른 스크립트에서 이 인스턴스에 접근하기 위한 프로퍼티
    public static GameManager Instance
    {
        get {
            if (instance == null) return null;
            return instance;
        }
    }


    void Update()
    {
        if (isFail) GameFail();
        else if (isClear) GameClear();
    }


    public void GameStart()
    {
        Time.timeScale = 1;
    }

    public void GamePause()
    {

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

    }
}
