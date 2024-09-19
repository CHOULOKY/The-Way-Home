using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class Goal : MonoBehaviour
{
    private int playerCount = 0;
    private List<int> playersInGoal = new List<int>(); // 목표 지점에 있는 플레이어의 PhotonView ID 저장

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PhotonView playerPV = collision.GetComponent<PhotonView>();

            if (playerPV != null && !playersInGoal.Contains(playerPV.ViewID))
            {
                playersInGoal.Add(playerPV.ViewID);
                playerCount++;
                Debug.Log("Player entered the goal. Total players in goal: " + playerCount);

                if (playerCount == 2)
                {
                    Debug.Log("Both players reached the goal!");
                    GameManager.Instance.GameClear();
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PhotonView playerPV = collision.GetComponent<PhotonView>();

            if (playerPV != null && playersInGoal.Contains(playerPV.ViewID))
            {
                playersInGoal.Remove(playerPV.ViewID);
                playerCount--;
                Debug.Log("Player left the goal. Total players in goal: " + playerCount);
            }
        }
    }
}

