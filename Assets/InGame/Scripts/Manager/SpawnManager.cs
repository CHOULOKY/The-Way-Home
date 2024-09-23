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

    // Client selects character in UI
    public void SpawnPlayer(string _name, Vector2 _point = default(Vector2))
    {
        if (_point == default(Vector2)) {
            _point = Vector2.zero;
        }

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
