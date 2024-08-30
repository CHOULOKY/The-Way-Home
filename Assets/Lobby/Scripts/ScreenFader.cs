using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Photon.Pun; // 추가
using UnityEngine.Events;

public class ScreenFader : MonoBehaviour
{
    public Image fadeImage;
    private void Awake()
    {
        fadeImage.enabled = true;
    }

    // 페이드 효과 (목표 알파 값 0이면 페이드 아웃, 1이면 페이드 인)
    [PunRPC]
    public IEnumerator Fade(float targetAlpha, float duration)
    {
        float startAlpha = fadeImage.color.a;
        float elapsedTime = 0f;

        Color color = fadeImage.color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);
            fadeImage.color = color;
            yield return null;
        }
        color.a = targetAlpha;
        fadeImage.color = color;
    }
}
