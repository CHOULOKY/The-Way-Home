using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CheckpointManager : MonoBehaviourPun
{
    private Vector3 lastCheckpointPosition;
    private string currentChapter;
    private bool checkpointSet = false;
    private int currentCheckpointNumber = -1;
    public PhotonView PV;

    private static CheckpointManager instance;

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
            SetCheckpoint(lastCheckpointPosition, currentCheckpointNumber);
        }
    }

    public void SetCheckpoint(Vector3 position, int checkpointNumber)
    {
        // 뒤에 있는 체크포인트 일 때만 체크포인트 갱신
        if (checkpointNumber > currentCheckpointNumber)
        {
            checkpointSet = true;
            PV.RPC("UpdateCheckpoint", RpcTarget.All, position, checkpointNumber);

            Debug.Log("Checkpoint set at: " + position + " with number: " + checkpointNumber);
        }
        
    }

    [PunRPC]
    private void UpdateCheckpoint(Vector3 position, int checkpointNumber)
    {
        if (checkpointNumber > currentCheckpointNumber)
        {
            lastCheckpointPosition = position;
            currentCheckpointNumber = checkpointNumber;
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

        Checkpoint[] checkpoints = FindObjectsOfType<Checkpoint>();
        foreach (Checkpoint checkpoint in checkpoints)
        {
            checkpoint.ResetCheckpoint();
        }

        Debug.Log("Checkpoint reset");
    }
}