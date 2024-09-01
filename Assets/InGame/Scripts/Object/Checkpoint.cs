using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Checkpoint : MonoBehaviour
{
    [Header("Checkpoint Settings")]
    public int checkpointNumber; // 체크포인트 번호, 뒤에 있을 수록 큰 번호 지정
    private bool girlPassed = false;
    private bool robotPassed = false;


    private void OnTriggerEnter2D(Collider2D collision)
    {       
        if (collision.CompareTag("Player"))
        {
            PhotonView playerPV = collision.GetComponent<PhotonView>();

            if (playerPV != null) {
                string selectedCharacter = (string)playerPV.Owner.CustomProperties["selectedCharacter"];

                if (selectedCharacter == "Girl" && !girlPassed)
                {
                    girlPassed = true;
                    Debug.Log("Girl player passed checkpoint number: " + checkpointNumber);

                }
                else if (selectedCharacter == "Robot" && !robotPassed)  
                {
                    robotPassed = true;
                    Debug.Log("Robot player passed checkpoint number: " + checkpointNumber);
                }

                if (girlPassed && robotPassed)
                    GameManager.Instance.checkpointManager.SetCheckpoint(transform.position, checkpointNumber);
            }


        }
    }

    public void ResetCheckpoint()
    {
        girlPassed = false;
        robotPassed = false;
    }
}
