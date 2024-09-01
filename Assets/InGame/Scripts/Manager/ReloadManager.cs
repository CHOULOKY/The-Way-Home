using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ReloadManager : MonoBehaviour
{
    public void ReloadChapter(Vector3 checkpointPosition)
    {
        StartCoroutine(ReloadChapterCoroutine(checkpointPosition));
    }

    private IEnumerator ReloadChapterCoroutine(Vector3 checkpointPosition)
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        PhotonView.Get(this).RPC("LoadGameScene", RpcTarget.Others, currentSceneName);

        yield return SceneManager.LoadSceneAsync(currentSceneName);

        CheckpointManager checkpointManager = FindObjectOfType<CheckpointManager>();
        if (checkpointManager != null)
        {
            //checkpointManager.RespawnAtCheckpointRPC(checkpointPosition);
        }
        else
        {
            Debug.LogWarning("CheckpointManager not found in the scene.");
        }
    }

    [PunRPC]
    private void LoadGameScene(string sceneName)
    {
        PhotonNetwork.LoadLevel(sceneName);
    }
}
