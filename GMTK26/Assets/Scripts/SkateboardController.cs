using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SkateboardController : MonoBehaviour
{
    [Header("Speed")]
    public float acceleration = 20f;
    public float maxSpeed = 8f;

    [Header("Turning")]
    public float turnSpeed = 180f;
    [Range(0f, 1f)]
    public float minTurnSpeedFactor = 0.25f;

    [Header("Feel")]
    public float grip = 8f;

    [Header("Ground")]
    public float groundCheckDistance = 0.6f;
    public float groundAlignSpeed = 10f;

    bool isGrounded;
    Vector3 groundNormal = Vector3.up;

    Rigidbody rb;
    PlayerInput playerInput;
    InputAction movementAction;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.maxLinearVelocity = maxSpeed;

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
        // For live tuning
        rb.maxLinearVelocity = maxSpeed;

        CheckGround();

        Vector2 input = movementAction.ReadValue<Vector2>();

        rb.AddForce(transform.forward * input.y * acceleration, ForceMode.Acceleration);

        float speedFactor = Mathf.Clamp01(rb.velocity.magnitude / maxSpeed);

        // Turn faster while moving, slower while not
        float turnFactor = Mathf.Lerp(minTurnSpeedFactor, 1f, speedFactor);
        float turnThisFrame = input.x * turnSpeed * turnFactor * Time.fixedDeltaTime;
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, turnThisFrame, 0f));

        // Smoother velocity realignment
        Vector3 flatVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        Vector3 targetVelocity = transform.forward * flatVelocity.magnitude;
        Vector3 carvedVelocity = Vector3.Lerp(flatVelocity, targetVelocity, grip * Time.fixedDeltaTime);
        rb.velocity = new Vector3(carvedVelocity.x, rb.velocity.y, carvedVelocity.z);

        if (isGrounded)
        {
            Quaternion alignedRotation = Quaternion.FromToRotation(transform.up, groundNormal) * rb.rotation;
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, alignedRotation, groundAlignSpeed * Time.fixedDeltaTime));
        }
    }

    void CheckGround()
    {
        Vector3 origin = transform.position + Vector3.up * 0.2f;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hitInfo, groundCheckDistance))
        {
            isGrounded = true;
            groundNormal = hitInfo.normal;
        }
        else
        {
            isGrounded = false;
            groundNormal = Vector3.up;
        }
    }
}
