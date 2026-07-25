using UnityEngine;
using UnityEngine.InputSystem;

public class SkateboardController : MonoBehaviour
{
    [Header("Speed")]
    public float acceleration = 25f;
    public float maxSpeed = 10f;

    [Header("Turning")]
    public float turnSpeed = 180f;
    [Range(0f, 1f)]
    public float minTurnSpeedFactor = 0.25f;

    [Header("Feel")]
    // drag for speed, grip for carving
    public float grip = 4f;
    public float drag = 2f;

    [Header("Ground")]
    // steepness cutoff
    public float groundNormalMinY = 0.1f;
    public float groundCheckDistance = 0.3f;

    [Header("Jump")]
    public float jumpForce = 8f;

    [Header("Gravity")]
    public float gravity = -20f;

    [Header("Sliding")]
    // how flat a surface needs to be before you stop sliding down it
    public float slideThresholdY = 0.9f;

    [Header("Visual Tilt")]
    // cosmetic tilt, not physical
    public Transform visualMesh;
    public float tiltSpeed = 10f;

    [Header("Air Pitch")]
    // for ollie
    public float maxAirPitch = 20f;
    public float airPitchVelocityRange = 10f;

    CharacterController controller;
    PlayerInput playerInput;
    InputAction movementAction;
    InputAction jumpAction;

    Vector3 velocity;
    bool isGrounded;
    bool wasGrounded;
    bool jumpQueued;
    Vector3 groundNormal = Vector3.up;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        movementAction = playerInput.actions["Movement"];
        jumpAction = playerInput.actions["Ollie"];
    }

    void Update()
    {
        CheckGround();

        Vector2 input = movementAction.ReadValue<Vector2>();
        if (jumpAction.WasPressedThisFrame())
        {
            jumpQueued = true;
        }

        HandleTurning(input);

        if (isGrounded)
        {
            HandleGroundedMovement(input);
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        jumpQueued = false;
        controller.Move(velocity * Time.deltaTime);
        UpdateVisualTilt();
    }

    void HandleTurning(Vector2 input)
    {
        float speedFactor = Mathf.Clamp01(velocity.magnitude / maxSpeed);
        float turnFactor = Mathf.Lerp(minTurnSpeedFactor, 1f, speedFactor);
        transform.Rotate(Vector3.up, input.x * turnSpeed * turnFactor * Time.deltaTime, Space.Self);
    }

    void HandleGroundedMovement(Vector2 input)
    {
        Vector3 slopeForward = Vector3.ProjectOnPlane(transform.forward, groundNormal).normalized;

        velocity += slopeForward * input.y * acceleration * Time.deltaTime;

        if (groundNormal.y < slideThresholdY)
        {
            Vector3 slopeGravity = Vector3.ProjectOnPlane(Vector3.up * gravity, groundNormal);
            velocity += slopeGravity * Time.deltaTime;
        }

        velocity *= Mathf.Clamp01(1f - drag * Time.deltaTime);

        if (Mathf.Abs(input.y) > 0.01f)
        {
            Vector3 targetVelocity = slopeForward * velocity.magnitude;
            Vector3 carved = Vector3.Lerp(velocity, targetVelocity, grip * Time.deltaTime);
            if (carved.y < velocity.y) carved.y = velocity.y;
            velocity = carved;
        }

        Vector3 horizontal = Vector3.ClampMagnitude(new Vector3(velocity.x, 0f, velocity.z), maxSpeed);
        velocity.x = horizontal.x;
        velocity.z = horizontal.z;

        if (jumpQueued) velocity.y += jumpForce;
    }

    void CheckGround()
    {
        float radius = controller.radius;
        Vector3 capsuleBottom = new Vector3(transform.position.x, controller.bounds.min.y, transform.position.z);
        Vector3 origin = capsuleBottom + Vector3.up * (radius + 0.05f);

        if (Physics.SphereCast(origin, radius * 0.9f, Vector3.down, out RaycastHit hit, groundCheckDistance)
            && hit.normal.y > groundNormalMinY)
        {
            isGrounded = true;
            groundNormal = hit.normal;
        }
        else
        {
            isGrounded = false;
            groundNormal = Vector3.up;
        }
    }

    void UpdateVisualTilt()
    {
        if (visualMesh == null) return;

        Quaternion targetLocalTilt;

        if (isGrounded)
        {
            Vector3 localNormal = transform.InverseTransformDirection(groundNormal);
            targetLocalTilt = Quaternion.FromToRotation(Vector3.up, localNormal);
        }
        else
        {
            float pitchT = Mathf.Clamp(velocity.y / airPitchVelocityRange, -1f, 1f);
            targetLocalTilt = Quaternion.Euler(-pitchT * maxAirPitch, 0f, 0f);
        }

        bool justLanded = isGrounded && !wasGrounded;
        visualMesh.localRotation = justLanded
            ? targetLocalTilt
            : Quaternion.Slerp(visualMesh.localRotation, targetLocalTilt, tiltSpeed * Time.deltaTime);

        wasGrounded = isGrounded;
    }
}