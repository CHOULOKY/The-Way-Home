using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager : MonoBehaviourPun
{
    private string selectedCharacter = (string)PhotonNetwork.LocalPlayer.CustomProperties["selectedCharacter"];
    private bool isHostReady = false;

    public PhotonView PV;

    private static ObjectManager instance;

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


    public void ReadyForSpawn()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(HostSpawnRoutine());
        }
        else
        {
            StartCoroutine(GuestSpawnRoutine());
        }
    }

    private IEnumerator HostSpawnRoutine()
    {
        // The host creates the character right away
        yield return null;
        SpawnPlayer(selectedCharacter);

        // The host is ready
        PV.RPC("HostReadyRPC", RpcTarget.Others);
    }

    private IEnumerator GuestSpawnRoutine()
    {
        // Guest waits until the host is ready
        yield return new WaitUntil(() => isHostReady);
        SpawnPlayer(selectedCharacter);
    }

    [PunRPC]
    private void HostReadyRPC()
    {
        isHostReady = true;
    }

    // Client selects character in the lobby
    public void SpawnPlayer(string selectedCharacter)
    {
        Vector3 spawnPosition;

        if (GameManager.Instance.checkpointManager.IsCheckpointSet())
            spawnPosition = GameManager.Instance.checkpointManager.GetLastCheckpointPosition();
        else
            spawnPosition = new Vector3(0, -0.5f, 0);

        StartCoroutine(SpawnPlayerRoutine(selectedCharacter, spawnPosition));
    }

    private IEnumerator SpawnPlayerRoutine(string selectedCharacter, Vector3 spawnPosition)
    {
        yield return new WaitForSeconds(0.1f);

        if (selectedCharacter == "Girl")
        {
            spawnPosition.x += 1;
            PhotonNetwork.Instantiate("Girl", spawnPosition, Quaternion.identity);
        }
        else if (selectedCharacter == "Robot")
        {
            spawnPosition.x -= 1;
            PhotonNetwork.Instantiate("Robot", spawnPosition, Quaternion.identity);
        }

        // 리스폰 하는 경우에만 카메라 재지정
        if (GameManager.Instance.checkpointManager.IsCheckpointSet())
            GameManager.Instance.mainCamera.StartSet();
    }

    // Client selects character in UI
    public void SpawnPlayer()
    {
        if (GameManager.Instance.uiManager.selected == "Girl") {
            PhotonNetwork.Instantiate("Girl", new Vector3(1, -1f, 0), Quaternion.identity);
        }
        else if (GameManager.Instance.uiManager.selected == "Robot") {
            PhotonNetwork.Instantiate("Robot", new Vector3(-1, -1f, 0), Quaternion.identity);
        }
    }

    public void DestroyPlayer()
    {
        PV.RPC("DestroyPlayerRPC", RpcTarget.All);
    }

    [PunRPC]
    public void DestroyPlayerRPC()
    {
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject playerObject in playerObjects)
        {
            PhotonView playerPhotonView = playerObject.GetComponent<PhotonView>();

            if (playerPhotonView != null && playerPhotonView.IsMine)
            {
                PhotonNetwork.RemoveRPCs(playerPhotonView);
                PhotonNetwork.Destroy(playerPhotonView);
            }
        }

        Debug.Log($"Player {selectedCharacter} has been destroyed on all clients.");
    }
}
