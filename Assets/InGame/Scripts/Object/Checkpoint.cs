using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Checkpoint Settings")]
    public int checkpointNumber; // 체크포인트 번호, 뒤에 있을 수록 큰 번호 지정



    private void OnTriggerEnter2D(Collider2D collision)
    {
        CheckpointManager checkpointManager = FindObjectOfType<CheckpointManager>();
        
        if (collision.CompareTag("Player"))
        {
            GameManager.Instance.checkpointManager.SetCheckpoint(transform.position, checkpointNumber);
        }
    }
}
