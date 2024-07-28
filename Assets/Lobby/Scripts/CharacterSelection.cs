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

    // ���õ� ĳ���͸� �����ϴ� ��ųʸ�
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
            Debug.LogError("SpriteRenderer ������Ʈ�� �����ϴ�.");
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
            Debug.LogError("Description ������Ʈ�� �Ҵ���� �ʾҽ��ϴ�.");
        }

        // �ʱ�ȭ �� �ٸ� Ŭ���̾�Ʈ�� ���� ���� �ݿ�
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
            // ���� ����
            DeselectCharacter();
        }
        else
        {
            // �̹� �ٸ� ĳ���͸� ������ ��� ���� �Ұ�
            if (clientsWithSelection.Contains(PhotonNetwork.LocalPlayer.ActorNumber))
            {
                Debug.Log($"Player {PhotonNetwork.LocalPlayer.ActorNumber} has already selected a character.");
                return;
            }
            // ����
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

        // �ٸ� Ŭ���̾�Ʈ���� ���� ���� �˸�
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

        // �ٸ� Ŭ���̾�Ʈ���� ���� ���� ���� �˸�
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

        // ĳ���� ���� ���� �� Ŭ�� ��Ȱ��ȭ
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

        // ĳ���� ���� �� Ŭ�� Ȱ��ȭ
        if (characterName == gameObject.name)
        {
            spriteRenderer.color = originalColor * 0.8f;
            GetComponent<CapsuleCollider2D>().enabled = true;
        }

        Debug.Log($"Character {characterName} deselected.");
    }

    // Player�� ���� ���� �� ȣ��Ǵ� �޼���
    public void HandlePlayerLeftRoom(Player otherPlayer)
    {
        /*
        // �ٸ� �÷��̾ ���� ������ �� �� �÷��̾ ������ ĳ���͸� �ʱ�ȭ
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
