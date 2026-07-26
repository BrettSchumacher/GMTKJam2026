using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public static List<Interactable> Interactives;
    
    public NpcPlacement NpcPlacement;
    public bool OneTimeInteractable = false;
    public bool DisableWhileWaitingForTrick = true;
    
    [SerializeField] protected string interactText;

    protected bool isPlayerInArea;
    private bool interacted = false;

    private bool overrideInteractable = false;

    private void OnEnable()
    {
        Interactives ??= new();
        Interactives.Add(this);
    }

    private void OnDisable()
    {
        Interactives.Remove(this);
    }

    protected void OnTriggerEnter(Collider other)
    {
        if (overrideInteractable || (OneTimeInteractable && interacted))
        {
            return;
        }
        
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
        if (overrideInteractable || (OneTimeInteractable && interacted))
        {
            return;
        }
        
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
                HideText();
            }
        }
    }

    private void HideText()
    {
        GameManager.gameManager.SetSelectedInteractableObj(null);
        GameManager.gameManager.SetInteractText("");
        isPlayerInArea = false;
    }

    public virtual void TriggerAction()
    {
        if (NpcPlacement)
        {
            NpcPlacement.Interact();
        }

        interacted = true;
        
        // Deactivate once we've gone through the conversation
        if (OneTimeInteractable)
        {
            HideText();
            enabled = false;
            var dampenField = GetComponent<DampenZone>();
            if (dampenField)
            {
                dampenField.enabled = false;
            }
        }
    }

    public void SetOverrideInteractable(bool interactable)
    {
        if (overrideInteractable == interactable)
        {
            return;
        }

        overrideInteractable = interactable;
        if (overrideInteractable && isPlayerInArea)
        {
            HideText();
        }
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
