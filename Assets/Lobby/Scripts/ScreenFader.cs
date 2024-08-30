using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Photon.Pun; // �߰�
using UnityEngine.Events;

public class ScreenFader : MonoBehaviour
{
    public Image fadeImage;
    private void Awake()
    {
        fadeImage.enabled = true;
    }

    // ���̵� ȿ�� (��ǥ ���� �� 0�̸� ���̵� �ƿ�, 1�̸� ���̵� ��)
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
