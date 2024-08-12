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
            PhotonNetwork.Instantiate("Player Girl", new Vector3(1, -0.5f, 0), Quaternion.identity);
        }
        else if (selectedCharacter == "Robot")
        {
            PhotonNetwork.Instantiate("Player Robot", new Vector3(-1, -0.5f, 0), Quaternion.identity);
        }
    }

    // Client selects character in UI
    public void SpawnPlayer()
    {
        if (GameManager.Instance.uiManager.selected == "Girl") {
            PhotonNetwork.Instantiate("Player Girl", new Vector3(1, -0.5f, 0), Quaternion.identity);
        }
        else if (GameManager.Instance.uiManager.selected == "Robot") {
            PhotonNetwork.Instantiate("Player Robot", new Vector3(-1, -0.5f, 0), Quaternion.identity);
        }
    }
}
