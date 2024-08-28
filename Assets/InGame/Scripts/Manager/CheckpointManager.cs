using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CheckpointManager : MonoBehaviour
{
    private Vector3 lastCheckpointPosition;
    private bool checkpointSet = false;
    private string currentChapter;
    private int currentCheckpointNumber = -1;
    public PhotonView PV;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string chapter = scene.name;

        if (currentChapter != chapter)
        {
            ResetCheckpoint();
            currentChapter = chapter;
        }

        if (IsCheckpointSet())
        {
            PV.RPC("UpdateCheckpoint", RpcTarget.All, lastCheckpointPosition, currentCheckpointNumber);
        }
    }

    public void SetCheckpoint(Vector3 position, int checkpointNumber)
    {
        // 뒤에 있는 체크포인트 일 때만 체크포인트 갱신
        if (checkpointNumber > currentCheckpointNumber)
        {
            lastCheckpointPosition = position;
            currentCheckpointNumber = checkpointNumber;
            checkpointSet = true;
            Debug.Log("Checkpoint set at: " + position + " with number: " + checkpointNumber);

            PV.RPC("UpdateCheckpoint", RpcTarget.All, position, checkpointNumber);
        }
        
    }

    [PunRPC]
    private void UpdateCheckpoint(Vector3 position, int checkpointNumber)
    {
        if (checkpointNumber > currentCheckpointNumber)
        {
            lastCheckpointPosition = position;
            currentCheckpointNumber = checkpointNumber;
            checkpointSet = true;
            Debug.Log("Checkpoint updated at: " + position + " with number: " + checkpointNumber);
        }
    }

    public Vector3 GetLastCheckpointPosition()
    {
        return checkpointSet ? lastCheckpointPosition : Vector3.zero;
    }

    public bool IsCheckpointSet()
    {
        return checkpointSet;
    }

    public void ResetCheckpoint()
    {
        lastCheckpointPosition = Vector3.zero;
        currentCheckpointNumber = -1;

        checkpointSet = false;
        Debug.Log("Checkpoint reset");
    }


    // 체크포인트에 리스폰
    public void RespawnAtCheckpoint()
    {
        if (IsCheckpointSet())
        {
            Vector3 checkpointPosition = GetLastCheckpointPosition();
            Debug.Log("Respawning at checkpoint: " + checkpointPosition);

            if (PV != null)
            {
                PV.RPC("RespawnAtCheckpointRPC", RpcTarget.All, checkpointPosition);

                // GameManager.Instance.reloadManager.ReloadChapter(checkpointPosition);
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
    public void RespawnAtCheckpointRPC(Vector3 checkpointPosition)
    {
        string selectedCharacter = (string)PhotonNetwork.LocalPlayer.CustomProperties["selectedCharacter"];
        GameManager.Instance.objectManager.SpawnPlayerAtPosition(selectedCharacter, checkpointPosition);
    }
}