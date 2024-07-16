using UnityEngine;

public class CharacterSelection: MonoBehaviour
{
    public GameObject descriptionObject;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Color highlightColor = Color.white;

    private bool isSelected = false;

    [SerializeField]
    private static string selectedCharacter;

    private Sprite originalSprite;

    private void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
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
    }

    private void OnMouseEnter()
    {
        descriptionObject.SetActive(true);
        spriteRenderer.color = highlightColor;
    }

    private void OnMouseExit()
    {
        descriptionObject.SetActive(false);
        spriteRenderer.color = originalColor * 0.8f;
    }

    private void OnMouseDown()
    {
        if (selectedCharacter != null && selectedCharacter != gameObject.name)
        {
            return;
        }
        if (isSelected)
        {
            // Deselect the character
            DeselectCharacter();
        }
        else
        {
            // Select the character
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
        selectedCharacter = gameObject.name;
        Debug.Log("Your Character : " +  selectedCharacter);
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
        selectedCharacter = null;
    }
}
