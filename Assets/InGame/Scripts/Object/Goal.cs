using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class Goal : MonoBehaviour
{
    private HashSet<int> playersInGoal = new HashSet<int>(); // 목표 지점에 있는 플레이어의 PhotonView ID 저장

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) {
            HandlePlayerEnter(collision);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) {
            HandlePlayerExit(collision);
        }
    }

    private void HandlePlayerEnter(Collider2D playerCollider)
    {
        PhotonView playerPV = playerCollider.GetComponent<PhotonView>();

        if (playerPV != null && playersInGoal.Add(playerPV.ViewID)) {
            // Debug.Log($"Player entered the goal. Total players in goal: {playerCount}");
            if (playersInGoal.Count >= 2) {
                // Debug.Log("Both players reached the goal!");
                GameManager.Instance.HandleGameClear();
            }
        }
    }

    private void HandlePlayerExit(Collider2D playerCollider)
    {
        PhotonView playerPV = playerCollider.GetComponent<PhotonView>();

        if (playerPV != null && playersInGoal.Remove(playerPV.ViewID)) {
            // Debug.Log($"Player left the goal. Total players in goal: {playerCount}");
        }
    }
}

