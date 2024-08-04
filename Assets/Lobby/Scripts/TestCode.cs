using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TestCode : MonoBehaviourPunCallbacks
{

    private void Start()
    {
        string selectedCharacter = (string)PhotonNetwork.LocalPlayer.CustomProperties["selectedCharacter"];
        if (selectedCharacter == "Girl")
        {
            PhotonNetwork.Instantiate("Player Girl", new Vector3(1, -0.5f, 0), Quaternion.identity);
        }
        else if (selectedCharacter == "Robot")
        {
            PhotonNetwork.Instantiate("Player Robot", new Vector3(-1, -0.5f, 0), Quaternion.identity);
        }

        Debug.Log("-> MainCamera: Player Found");
    }
}
