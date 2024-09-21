using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviourPun
{
    // Client selects character in UI
    public void SpawnPlayer(string _name, Vector2 _point = default(Vector2))
    {
        if (_point == default(Vector2)) {
            _point = Vector2.zero;
        }

        if (_name == "Girl") {
            PhotonNetwork.Instantiate("Girl", _point, Quaternion.identity);
        }
        else if (_name == "Robot") {
            PhotonNetwork.Instantiate("Robot", _point, Quaternion.identity);
        }
    }
}
