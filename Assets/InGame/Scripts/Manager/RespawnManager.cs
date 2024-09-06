using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RespawnManager : MonoBehaviourPun
{

    public PhotonView PV;

    private static RespawnManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else if (instance != this)
        {
            Destroy(this.gameObject);
        }
    }

    // 체크포인트에 리스폰
    public void RespawnAtCheckpoint()
    {
        if (GameManager.Instance.checkpointManager.IsCheckpointSet())
        {
            if (PV != null)
            {
                PV.RPC("ReloadAndRespawn", RpcTarget.All);
            }
            else
            {
                Debug.LogWarning("PhotonView is either null or not owned by this client.");
            }
        }
        else
        {
            Debug.LogWarning("No checkpoint set.");
        }
        
    }

    [PunRPC]
    public void ReloadAndRespawn()
    {
        StartCoroutine(ReloadAndRespawnRoutine());
    }

    private IEnumerator ReloadAndRespawnRoutine()
    {
        GameManager.Instance.ReloadScene();

        yield return new WaitForSeconds(0.5f);
        while (!SceneManager.GetActiveScene().isLoaded)
        {
            yield return null;
        }

        GameManager.Instance.objectManager.ReadyForSpawn();
        
        GameManager.Instance.GameStart();

        // 체크포인트 위치로 플레이어 이동
        Vector3 checkpointPosition = GameManager.Instance.checkpointManager.GetLastCheckpointPosition();
        string selectedCharacter = (string)PhotonNetwork.LocalPlayer.CustomProperties["selectedCharacter"];
        GameManager.Instance.objectManager.SpawnPlayerAtPosition(selectedCharacter, checkpointPosition);

        Debug.Log("Respawned at checkpoint: " + checkpointPosition);
    }
}
