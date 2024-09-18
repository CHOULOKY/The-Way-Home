using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("RoomSet")]
    public string selected;

    [Header("Start")]
    public GameObject AccessPanel;
    public Text ConnectText;

    [Header("Pause")]
    public GameObject PauseButton;

    [Header("Fail")]
    public GameObject FailButton;

    [Header("Clear")]
    public GameObject ClearButton;

    [Header("Chapter Clear UI")]
    public CanvasGroup ChapterClearUI;

    void Start()
    {
        // If the client has selected a character
        if (GameManager.Instance.hasSelectedCharacterInLobby)
        {
            GameStart();
        }
        else
        {
            AccessPanel.SetActive(true);
        }
    }
    private static UIManager instance;

    private void Awake()
    {
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


    public void SetCharacter(string player)
    {
        ConnectText.text = selected = player;
    }

    public void GameRoom()
    {
        AccessPanel.SetActive(true);
    }

    public void GameStart()
    {
        AccessPanel.SetActive(false);
    }

    public void GamePause()
    {

    }

    public void GameClear()
    {
        ShowChapterCompleteUI(1.0f);
    }


    public void ShowChapterCompleteUI(float duration)
    {
        Debug.Log("Start UI Coroutine");
        /*
        Canvas chapterCanvas = ChapterClearUI.GetComponent<Canvas>();
        if (chapterCanvas != null)
        {
            chapterCanvas.sortingOrder = 10; // 가장 앞에 보이도록 설정
        }*/
        ChapterClearUI.gameObject.SetActive(true);
        StartCoroutine(FadeInCoroutine(ChapterClearUI, duration));
    }

    private IEnumerator FadeInCoroutine(CanvasGroup uiElement, float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            uiElement.alpha = Mathf.Clamp01(elapsedTime / duration);
            yield return null;
        }
        uiElement.alpha = 1f;
    }
}
