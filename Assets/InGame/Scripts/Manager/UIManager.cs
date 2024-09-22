using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Start")]
    public GameObject AccessPanel;
    public Text ConnectText;
    public string selected;

    [Header("Start")]
    public CanvasGroup ClearUI;

    public void SetCharacter(string player)
    {
        ConnectText.text = selected = player;
    }

    public string GameStart()
    {
        AccessPanel.SetActive(false);
        return selected;
    }

    public void GameClear()
    {
        ShowChapterCompleteUI(3.0f);
    }

    public void ShowChapterCompleteUI(float duration)
    {
        Debug.Log("Start UI Coroutine");
        ClearUI.gameObject.SetActive(true);
        StartCoroutine(FadeInCoroutine(ClearUI, duration));
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
