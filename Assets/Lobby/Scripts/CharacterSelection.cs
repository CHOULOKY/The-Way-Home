using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System.Collections.Generic;

public class CharacterSelection : MonoBehaviourPunCallbacks
{
    public GameObject descriptionObject;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Color highlightColor = Color.white;
    private Color otherSelectedColor = Color.red;

    private bool isSelected = false;

    // 선택된 캐릭터를 추적하는 딕셔너리
    private static Dictionary<string, int> selectedCharacters = new Dictionary<string, int>();
    private static HashSet<int> clientsWithSelection = new HashSet<int>();

    private Sprite originalSprite;
    private PhotonView PV;

    private void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        PV = GetComponent<PhotonView>();
        animator.enabled = false;

        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer 컴포넌트가 없습니다.");
        }
        else
        {
            originalColor = spriteRenderer.color;
            originalSprite = spriteRenderer.sprite;
            spriteRenderer.color = originalColor * 0.8f;
        }

        if (descriptionObject != null)
        {
            descriptionObject.SetActive(false);
        }
        else
        {
            Debug.LogError("Description 오브젝트가 할당되지 않았습니다.");
        }

        // 초기화 시 다른 클라이언트의 선택 상태 반영
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
            // 선택 해제
            DeselectCharacter();
        }
        else
        {
            // 이미 다른 캐릭터를 선택한 경우 선택 불가
            if (clientsWithSelection.Contains(PhotonNetwork.LocalPlayer.ActorNumber))
            {
                Debug.Log($"Player {PhotonNetwork.LocalPlayer.ActorNumber} has already selected a character.");
                return;
            }
            // 선택
            SelectCharacter();
        }
    }

    private void SelectCharacter()
    {
        if (animator != null)
        {
            animator.enabled = true;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = highlightColor;
        }

        isSelected = true;
        selectedCharacters[gameObject.name] = PhotonNetwork.LocalPlayer.ActorNumber;
        clientsWithSelection.Add(PhotonNetwork.LocalPlayer.ActorNumber);
        Debug.Log($"Player {PhotonNetwork.LocalPlayer.ActorNumber} selected: {gameObject.name}");

        // 다른 클라이언트에게 선택 상태 알림
        PV.RPC("UpdateCharacterSelection", RpcTarget.OthersBuffered, gameObject.name, PhotonNetwork.LocalPlayer.ActorNumber);
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

        // 다른 클라이언트에게 선택 해제 상태 알림
        PV.RPC("UpdateCharacterDeselection", RpcTarget.OthersBuffered, gameObject.name);
    }

    [PunRPC]
    private void UpdateCharacterSelection(string characterName, int playerID)
    {
        if (!selectedCharacters.ContainsKey(characterName))
        {
            selectedCharacters[characterName] = playerID;
            clientsWithSelection.Add(playerID);
        }

        // 캐릭터 색상 변경 및 클릭 비활성화
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

        // 캐릭터 색상 및 클릭 활성화
        if (characterName == gameObject.name)
        {
            spriteRenderer.color = originalColor * 0.8f;
            GetComponent<CapsuleCollider2D>().enabled = true;
        }

        Debug.Log($"Character {characterName} deselected.");
    }

    // Player가 방을 떠날 때 호출되는 메서드
    public void HandlePlayerLeftRoom(Player otherPlayer)
    {
        /*
        // 다른 플레이어가 방을 나갔을 때 그 플레이어가 선택한 캐릭터를 초기화
        foreach (var character in selectedCharacters)
        {
            if (character.Value == otherPlayer.ActorNumber)
            {
                PV.RPC("UpdateCharacterDeselection", RpcTarget.AllBuffered, character.Key);
            }
        }
        */
    }
}
