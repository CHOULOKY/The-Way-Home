using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Start")]
    public GameObject AccessPanel;
    public Text ConnectText;
    public string selected;

    [Header("Clear")]
    public CanvasGroup ClearUI;

    [Header("Fade")]
    public CanvasGroup FadeCanvas;

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
        // Debug.Log("Start UI Coroutine");
        Player[] players = FindObjectsOfType<Player>();
        if (players.Length > 0) {
            foreach (Player player in players) {
                Girl girl = player.GetComponent<Girl>();
                if (girl) {
                    girl.UICanvas.SetActive(false);
                }
                else {
                    Robot robot = player.GetComponent<Robot>();
                    if (robot) {
                        robot.UICanvas.SetActive(false);
                    }
                }
            }
        }

        ClearUI.gameObject.SetActive(true);
        StartCoroutine(FadeInCoroutine(ClearUI, 3.0f));
    }

    public IEnumerator FadeInCoroutine(CanvasGroup uiElement, float duration)
    {
        if (uiElement == null) {
            uiElement = FadeCanvas;
        }

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            uiElement.alpha = Mathf.Clamp01(elapsedTime / duration);
            yield return null;
        }
        uiElement.alpha = 1f;
    }

    public IEnumerator FadeOutCoroutine(CanvasGroup uiElement, float duration)
    {
        if (uiElement == null) {
            uiElement = FadeCanvas;
        }

        float elapsedTime = 0f;
        while (elapsedTime < duration) {
            elapsedTime += Time.deltaTime;
            uiElement.alpha = Mathf.Clamp01(1f - (elapsedTime / duration));
            yield return null;
        }

        uiElement.alpha = 0f;
    }

}
