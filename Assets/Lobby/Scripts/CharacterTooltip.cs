using UnityEngine;

public class CharacterTooltip : MonoBehaviour
{
    public GameObject descriptionObject;

    private void Start()
    {
        if (descriptionObject != null)
        {
            descriptionObject.SetActive(false);
        }
        else
        {
            Debug.LogError("Description object is not assigned.");
        }
    }

    private void OnMouseEnter()
    {
        if (descriptionObject != null)
        {
            descriptionObject.SetActive(true);
        }
    }

    private void OnMouseExit()
    {
        if (descriptionObject != null)
        {
            descriptionObject.SetActive(false);
        }
    }
}
