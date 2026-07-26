using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    [SerializeField] protected string interactText;

    protected bool isPlayerInArea;

    protected void OnTriggerEnter(Collider other)
    {
        // If Player enters area, set this interactable object as the selected object and set interact text
        if (other.GetComponent<SkateboardController>() != null)
        {
            GameManager.gameManager.SetSelectedInteractableObj(this);
            GameManager.gameManager.SetInteractText(interactText);
            isPlayerInArea = true;
        }
    }

    // Probably don't need this, but I copied it from another project so why not.
    protected void OnTriggerStay(Collider other)
    {
        // If the Player is in the area when they left the area of another interactable object (or it disappeared), set this as the selected interactable object
        if (other.GetComponent<SkateboardController>() != null && GameManager.gameManager.GetSelectedInteractableObj() == null)
        {
            GameManager.gameManager.SetSelectedInteractableObj(this);
            GameManager.gameManager.SetInteractText(interactText);
            isPlayerInArea = true;
        }
    }

    protected void OnTriggerExit(Collider other)
    {
        // If Player leaves area
        if (other.GetComponent<SkateboardController>() != null)
        {
            if (GameManager.gameManager.GetSelectedInteractableObj() == this)
            {
                GameManager.gameManager.SetSelectedInteractableObj(null);
                GameManager.gameManager.SetInteractText("");
                isPlayerInArea = false;
            }
        }
    }

    public virtual void TriggerAction()
    {

    }

    protected void OnDestroy()
    {
        if (isPlayerInArea)
        {
            GameManager.gameManager.SetSelectedInteractableObj(null);
            GameManager.gameManager.SetInteractText("");
        }
    }
}
