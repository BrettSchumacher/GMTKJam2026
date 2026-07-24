using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SkateboardController : MonoBehaviour
{

    Rigidbody rb;
    public float forwardForce = 20f;
    public float torque = 4f;

    [SerializeField]
    private PlayerInput playerInput;
    private InputAction movementAction;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.maxLinearVelocity = 20;

        InputSetup();
    }

    void InputSetup()
    {
        // Setup movement actions
        playerInput = GetComponent<PlayerInput>();
        movementAction = playerInput.actions["Move"];
    }

    void FixedUpdate()
    {
        if(movementAction.ReadValue<Vector2>().y > 0)
        {
            rb.AddForce(forwardForce * transform.forward);
        }
        if(movementAction.ReadValue<Vector2>().y < 0)
        {
            rb.AddForce(forwardForce * -transform.forward);
        }
        if(movementAction.ReadValue<Vector2>().x > 0)
        {
            rb.AddTorque(Vector3.up * torque);
        }
        if(movementAction.ReadValue<Vector2>().x < 0)
        {
            rb.AddTorque(Vector3.up * -torque);
        }

    }
}
