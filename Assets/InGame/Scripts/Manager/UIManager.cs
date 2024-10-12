using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Start")]
    public GameObject accessPanel;
    public Text connectText;
    public string selectedCharacter;

    [Header("Clear")]
    public CanvasGroup clearUI;

    [Header("Fade")]
    public CanvasGroup fadeCanvas;

    public void SetCharacter(string player)
    {
        selectedCharacter = player;
        connectText.text = player;
    }

    public string StartGame()
    {
        accessPanel.SetActive(false);
        return selectedCharacter;
    }

    public void ShowGameClearUI()
    {
        clearUI.gameObject.SetActive(true);
        StartCoroutine(FadeInCoroutine(clearUI, 3.0f));
    }

    public IEnumerator FadeInCoroutine(CanvasGroup uiElement = null, float duration = 1.0f)
    {
        HidePlayerUICanvas();

        uiElement = uiElement != null ? uiElement : fadeCanvas;

        yield return FadeCoroutine(uiElement, duration, 0f, 1f);
    }

    public IEnumerator FadeOutCoroutine(CanvasGroup uiElement = null, float duration = 1.0f)
    {
        uiElement = uiElement != null ? uiElement : fadeCanvas;

        yield return FadeCoroutine(uiElement, duration, 1f, 0f);
    }

    private void HidePlayerUICanvas()
    {
        foreach (Player player in FindObjectsOfType<Player>()) {
            GameObject uiCanvas = player.GetComponent<Girl>()?.UICanvas
                ?? player.GetComponent<Robot>()?.UICanvas;
            if (uiCanvas != null) {
                uiCanvas.SetActive(false);
            }
        }
    }

    private IEnumerator FadeCoroutine(CanvasGroup uiElement, float duration, float startAlpha, float endAlpha)
    {
        float elapsedTime = 0f;
        uiElement.alpha = startAlpha;

        while (elapsedTime < duration) {
            elapsedTime += Time.deltaTime;
            uiElement.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            yield return null;
        }
        uiElement.alpha = endAlpha;
    }
}
