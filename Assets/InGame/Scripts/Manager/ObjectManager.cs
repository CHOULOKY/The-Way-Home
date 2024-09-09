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
        if (string.IsNullOrEmpty(selectedCharacter))
        {
            Debug.LogError("selectedCharacter is null or empty");
            return;
        }

        if (selectedCharacter == "Girl")
        {
            PhotonNetwork.Instantiate("Girl", new Vector3(1, -0.5f, 0), Quaternion.identity);
        }
        else if (selectedCharacter == "Robot")
        {
            PhotonNetwork.Instantiate("Robot", new Vector3(-1, -0.5f, 0), Quaternion.identity);
        }
    }

    public void SpawnPlayerAtPosition(string selectedCharacter, Vector3 checkpointPosition)
    {
        PV.RPC("SpawnPlayerAtPositionRPC", RpcTarget.All, selectedCharacter, checkpointPosition);
    }

    [PunRPC]
    public void SpawnPlayerAtPositionRPC(string selectedCharacter, Vector3 checkpointPosition)
    {
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject playerObject in playerObjects)
        {
            PhotonView playerPhotonView = playerObject.GetComponent<PhotonView>();
            
            if (playerPhotonView != null && playerPhotonView.IsMine)
            {
                PhotonNetwork.Destroy(playerPhotonView);
            }
        }

        StartCoroutine(InstantiatePlayerAfterDelay(selectedCharacter, checkpointPosition));
    }

    private IEnumerator InstantiatePlayerAfterDelay(string selectedCharacter, Vector3 checkpointPosition)
    {
        yield return new WaitForSeconds(0.1f);

        if (selectedCharacter == "Girl")
        {
            checkpointPosition.x += 1; 
            PhotonNetwork.Instantiate("Girl", checkpointPosition, Quaternion.identity);
        }
        else if (selectedCharacter == "Robot")
        {
            checkpointPosition.x -= 1; 
            PhotonNetwork.Instantiate("Robot", checkpointPosition, Quaternion.identity);
        }

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
}
