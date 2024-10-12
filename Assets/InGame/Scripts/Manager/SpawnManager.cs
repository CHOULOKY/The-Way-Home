using Photon.Pun;
using UnityEngine;

public class SpawnManager : MonoBehaviourPun
{
    private const string GIRL_CHARACTER = "Girl";
    private const string ROBOT_CHARACTER = "Robot";

    public void SpawnPlayer(string characterName, Vector2 spawnPoint = default)
    {
        spawnPoint = spawnPoint == default ? Vector2.zero : spawnPoint;
        characterName = string.IsNullOrEmpty(characterName) ? 
            (string)PhotonNetwork.LocalPlayer.CustomProperties["selectedCharacter"] : characterName;

        if (PhotonNetwork.IsMasterClient) {
            SpawnCharacter(characterName, spawnPoint);
            string guestCharacterName = characterName == GIRL_CHARACTER ? ROBOT_CHARACTER : GIRL_CHARACTER;
            photonView.RPC(nameof(SpawnGuestCharacter), RpcTarget.OthersBuffered, guestCharacterName, spawnPoint);
        }
    }

    [PunRPC]
    public void SpawnGuestCharacter(string characterName, Vector2 spawnPoint)
    {
        SpawnCharacter(characterName, spawnPoint);
    }

    // Client selects character in UI
    public void SpawnCharacter(string characterName, Vector2 spawnPoint)
    {
        if (characterName == GIRL_CHARACTER) {
            PhotonNetwork.Instantiate(GIRL_CHARACTER, spawnPoint, Quaternion.identity);
        } else if (characterName == ROBOT_CHARACTER) {
            PhotonNetwork.Instantiate(ROBOT_CHARACTER, spawnPoint, Quaternion.identity);
        } else {
            Debug.LogWarning($"Unknown character type: {characterName}");
        }
    }
}
