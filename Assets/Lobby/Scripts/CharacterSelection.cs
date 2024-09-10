using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSelection : MonoBehaviourPunCallbacks
{
    public GameObject descriptionObject;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Color highlightColor = Color.white;
    private Color otherSelectedColor = Color.red;

    private bool isSelected = false;

    // A dictionary that tracks the selected characters
    private static Dictionary<string, int> selectedCharacters = new Dictionary<string, int>();
    private static HashSet<int> clientsWithSelection = new HashSet<int>();

    private PhotonView PV;

    private void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        PV = GetComponent<PhotonView>();
        animator.enabled = false;

        if (originalColor == default(Color))
        {
            originalColor = spriteRenderer.color;
        }

        InitializeCharacterState();
    }

    private void InitializeCharacterState()
    {
        spriteRenderer.color = originalColor * 0.8f;
        descriptionObject.SetActive(false);

        // Reflecting other clients' selection status
        foreach (var character in selectedCharacters)
        {
            if (character.Value != PhotonNetwork.LocalPlayer.ActorNumber)
            {
                UpdateCharacterSelection(character.Key, character.Value);
            }
        }
    }

    private void OnMouseEnter()
    {
        descriptionObject.SetActive(true);
        spriteRenderer.color = highlightColor;
    }

    private void OnMouseExit()
    {
        descriptionObject.SetActive(false);
        if (!isSelected && (!selectedCharacters.ContainsKey(gameObject.name) || selectedCharacters[gameObject.name] != PhotonNetwork.LocalPlayer.ActorNumber))
        {
            spriteRenderer.color = originalColor * 0.8f;
        }
    }

    private void OnMouseDown()
    {
        if (selectedCharacters.ContainsKey(gameObject.name) && selectedCharacters[gameObject.name] != PhotonNetwork.LocalPlayer.ActorNumber)
        {
            Debug.Log($"Character {gameObject.name} is already selected by another player.");
            return;
        }

        if (isSelected)
        {
            // Deselect character
            DeselectCharacter();
        }
        else
        {
            // Cannot be selected if another character has already been selected
            if (clientsWithSelection.Contains(PhotonNetwork.LocalPlayer.ActorNumber))
            {
                Debug.Log($"Player {PhotonNetwork.LocalPlayer.ActorNumber} has already selected a character.");
                return;
            }
            // Select Character
            SelectCharacter();
        }
    }

    private void SelectCharacter()
    {
        if (animator != null)
        {
            animator.enabled = true;
            animator.SetBool("isGround", true);
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = highlightColor;
        }

        isSelected = true;
        selectedCharacters[gameObject.name] = PhotonNetwork.LocalPlayer.ActorNumber;
        clientsWithSelection.Add(PhotonNetwork.LocalPlayer.ActorNumber);
        Debug.Log($"Player {PhotonNetwork.LocalPlayer.ActorNumber} selected: {gameObject.name}");

        // Notify other clients of selection status
        PV.RPC("UpdateCharacterSelection", RpcTarget.OthersBuffered, gameObject.name, PhotonNetwork.LocalPlayer.ActorNumber);

        // Save selected characters to Custom Properties
        ExitGames.Client.Photon.Hashtable customProperties = PhotonNetwork.LocalPlayer.CustomProperties;
        customProperties["selectedCharacter"] = gameObject.name;
        PhotonNetwork.LocalPlayer.SetCustomProperties(customProperties);
        // 캐릭터 선택 상태 변경 알림
        FindObjectOfType<LobbyManager>().UpdateGameButton();
    }

    private void DeselectCharacter()
    {
        if (animator != null)
        {
            animator.Rebind();
            animator.enabled = false;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor * 0.8f;
        }

        isSelected = false;
        selectedCharacters.Remove(gameObject.name);
        clientsWithSelection.Remove(PhotonNetwork.LocalPlayer.ActorNumber);
        Debug.Log($"Player {PhotonNetwork.LocalPlayer.ActorNumber} deselected: {gameObject.name}");

        if (!PhotonNetwork.IsMasterClient)
        {
            FindObjectOfType<LobbyManager>().PV.RPC("GuestReady", RpcTarget.MasterClient, false);
        }

        // Notify other clients of deselect
        PV.RPC("UpdateCharacterDeselection", RpcTarget.OthersBuffered, gameObject.name);

        // Remove selected characters from Custom Properties
        ExitGames.Client.Photon.Hashtable customProperties = PhotonNetwork.LocalPlayer.CustomProperties;
        customProperties.Remove("selectedCharacter");
        PhotonNetwork.LocalPlayer.SetCustomProperties(customProperties);

        FindObjectOfType<LobbyManager>().UpdateGameButton();

    }

    [PunRPC]
    private void UpdateCharacterSelection(string characterName, int playerID)
    {
        if (!selectedCharacters.ContainsKey(characterName))
        {
            selectedCharacters[characterName] = playerID;
            clientsWithSelection.Add(playerID);
        }

        // Change character color and disable click
        if (characterName == gameObject.name)
        {
            spriteRenderer.color = otherSelectedColor;
            GetComponent<CapsuleCollider2D>().enabled = false;
        }

        Debug.Log($"Player {playerID} selected: {characterName}");
    }

    [PunRPC]
    private void UpdateCharacterDeselection(string characterName)
    {
        if (selectedCharacters.ContainsKey(characterName))
        {
            int playerID = selectedCharacters[characterName];
            selectedCharacters.Remove(characterName);
            clientsWithSelection.Remove(playerID);
        }

        // Character color and click activation
        if (characterName == gameObject.name)
        {
            spriteRenderer.color = originalColor * 0.8f;
            GetComponent<CapsuleCollider2D>().enabled = true;
        }

        Debug.Log($"Character {characterName} deselected.");
    }

    public bool IsCharacterSelected(int actorNumber)
    {
        return selectedCharacters.ContainsValue(actorNumber);
    }

    // Player가 방을 떠날 때 호출되는 메서드
    public void HandlePlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        // 나간 플레이어가 선택했던 캐릭터들에 대해 선택 해제를 수행
        foreach (var character in new Dictionary<string, int>(selectedCharacters))
        {
            if (character.Value == otherPlayer.ActorNumber)  // 나간 플레이어의 캐릭터 선택을 찾음
            {
                DeselectCharacter();
                PV.RPC("UpdateCharacterDeselection", RpcTarget.AllBuffered, character.Key);

                spriteRenderer.color = originalColor;
                animator.Rebind();
                animator.enabled = false;

                isSelected = false;
            }
        }
    }
}
