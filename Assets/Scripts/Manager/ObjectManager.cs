using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager : MonoBehaviour
{
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
