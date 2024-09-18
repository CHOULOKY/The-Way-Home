using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class PropsGoal : MonoBehaviour
{
    //public float fadeDuration = 1.0f;

    // Ʈ���� �ȿ� ���� ������Ʈ�� ���� ����
    private HashSet<string> playersInTrigger = new HashSet<string>();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PhotonView playerPV = collision.GetComponent<PhotonView>();

            if (playerPV != null)
            {
                string selectedCharacter = (string)playerPV.Owner.CustomProperties["selectedCharacter"];

                // �ߺ����� �ʰ� �÷��̾� �߰�
                if (!playersInTrigger.Contains(selectedCharacter))
                {
                    playersInTrigger.Add(selectedCharacter);
                    Debug.Log(selectedCharacter + " player reached the goal.");
                }

                // Girl�� Robot ��� �����ߴ��� Ȯ��
                if (playersInTrigger.Contains("Girl") && playersInTrigger.Contains("Robot"))
                {
                    Debug.Log("All Players entered the goal area.");
                    GameManager.Instance.GameClear();
                }
            }
        }
    }

}
