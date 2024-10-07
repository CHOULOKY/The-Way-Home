using Photon.Pun;
using UnityEngine;

public class SpawnManager : MonoBehaviourPun
{
    private GameObject girlCharater;
    private GameObject robotCharater;

    public void SpawnPlayer(string _name, Vector2 _point = default)
    {
        if (_point == default) {
            _point = Vector2.zero;
        }
        _name = _name == "" ? (string)PhotonNetwork.LocalPlayer.CustomProperties["selectedCharacter"] : _name;
        SpawnByType(_name, _point);
    }

    // Client selects character in UI
    public void SpawnByType(string _name, Vector2 _point = default)
    {
        if (_name == "Girl") {
            girlCharater = PhotonNetwork.Instantiate("Girl", _point, Quaternion.identity);
        }
        else if (_name == "Robot") {
            robotCharater = PhotonNetwork.Instantiate("Robot", _point, Quaternion.identity);
        }
    }
}
