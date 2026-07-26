using UnityEngine;
using UnityEngine.InputSystem;

public class StateTransite : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private InputActionReference movementAction;
    [SerializeField] private InputActionReference JumpAction;





    private void Update()
    {
        Vector2 movement = movementAction.action.ReadValue<Vector2>();
        animator.SetFloat("MoveY", movement.y);
        if (JumpAction.action.WasPressedThisFrame())
        {
            animator.SetTrigger("Jump");
        }
    }
       

    }

