using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class PropsGoal : MonoBehaviour
{
    //public float fadeDuration = 1.0f;

    // 트리거 안에 들어온 오브젝트의 수를 추적
    private HashSet<string> playersInTrigger = new HashSet<string>();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PhotonView playerPV = collision.GetComponent<PhotonView>();

            if (playerPV != null)
            {
                string selectedCharacter = (string)playerPV.Owner.CustomProperties["selectedCharacter"];

                // 중복되지 않게 플레이어 추가
                if (!playersInTrigger.Contains(selectedCharacter))
                {
                    playersInTrigger.Add(selectedCharacter);
                    Debug.Log(selectedCharacter + " player reached the goal.");
                }

                // Girl과 Robot 모두 도착했는지 확인
                if (playersInTrigger.Contains("Girl") && playersInTrigger.Contains("Robot"))
                {
                    Debug.Log("All Players entered the goal area.");
                    GameManager.Instance.GameClear();
                }
            }
        }
    }

}
