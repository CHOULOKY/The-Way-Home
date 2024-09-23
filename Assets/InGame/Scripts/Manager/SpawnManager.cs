using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class SpawnManager : MonoBehaviourPun
{
    private GameObject girlCharater;
    private GameObject robotCharater;
    private bool isHostReady = false;

    public void CheckSpawn(string _name, Vector2 _point = default(Vector2))
    {
        if (_point == default(Vector2)) {
            _point = Vector2.zero;
        }

        if (_name == "") {
            _name = (string)PhotonNetwork.LocalPlayer.CustomProperties["selectedCharacter"];
            LobbyToGame(_name, _point);
        }
        else {
            SpawnPlayer(_name, _point);
        } 
    }

    public void LobbyToGame(string _name, Vector2 _point)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(HostSpawnRoutine(_name, _point));
        }
        else
        {
            StartCoroutine(GuestSpawnRoutine(_name, _point));
        }
    }

    private IEnumerator HostSpawnRoutine(string _name, Vector2 _point)
    {
        SpawnPlayer(_name, _point);

        GameManager.Instance.GetComponent<PhotonView>().RPC("HostReadyRPC", RpcTarget.Others);
        
        yield break;
    }

    private IEnumerator GuestSpawnRoutine(string _name, Vector2 _point)
    {
        yield return new WaitUntil(() => isHostReady);
        
        SpawnPlayer(_name, _point);
    }

    [PunRPC]
    private void HostReadyRPC()
    {
        isHostReady = true;
    }


    // Client selects character in UI
    public void SpawnPlayer(string _name, Vector2 _point = default(Vector2))
    {
        if (_name == "Girl") {
            girlCharater = PhotonNetwork.Instantiate("Girl", _point, Quaternion.identity);
        }
        else if (_name == "Robot") {
            robotCharater = PhotonNetwork.Instantiate("Robot", _point, Quaternion.identity);
        }
    }

    public void DisableAllPlayers()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            player.SetActive(false);
        }
    }
}
