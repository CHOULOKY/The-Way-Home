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
        if (!GameManager.Instance.checkpointManager.IsCheckpointSet())
        {
            Debug.LogWarning("No checkpoint set.");
        }
        
        if (PV != null)
        {
            PV.RPC("ReloadAndRespawn", RpcTarget.All);
        }
        else
        {
            Debug.LogWarning("PhotonView is either null or not owned by this client.");
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
    }
}
