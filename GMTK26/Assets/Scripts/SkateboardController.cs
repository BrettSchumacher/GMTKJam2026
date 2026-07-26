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
    
    [Header("Camera FOV")]
    public float minFov = 60f;
    public float maxFov = 90f;
    public AnimationCurve fovCurve;

    [Header("Air Pitch")]
    // for ollie
    public float maxAirPitch = 20f;
    public float airPitchVelocityRange = 10f;

    [Header("Turn Lean")]
    public float maxTurnLean = 10f;

    [Header("Grinding")]
    public float grindAttachRadius = 1f;
    public float minGrindSpeed = 4f;
    public float maxGrindSpeed = 14f;
    public float grindReattachDelay = 0.3f;
    public float grindTurnSpeed = 8f;

    [Header("Dampening")]
    public float dampenDecaySpeed = 5f;
    public float dampenTimeout = 0.1f;
    [Range(0f, 1f)]
    public float dampenedSpeedFactor = 0.2f;

    public CameraShake cameraShake;

    CharacterController controller;
    PlayerInput playerInput;
    InputAction movementAction;
    InputAction jumpAction;
    InputAction grindAction;

    Vector3 velocity;
    bool isGrounded;
    bool wasGrounded;
    bool jumpQueued;
    Vector3 groundNormal = Vector3.up;
    float lastTurnAmount;
    bool isGrinding;
    GrindRail currentRail;
    float railDistance;
    float grindSpeed;
    float reattachTimer;
    float dampener;
    float lastDampenValue;
    float lastDampenRequestTime = float.NegativeInfinity;
    Vector3 lastDampenSourcePosition;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        movementAction = playerInput.actions["Movement"];
        jumpAction = playerInput.actions["Ollie"];
        grindAction = playerInput.actions["Grind"];
    }

    void Update()
    {
        bool requestIsFresh = Time.time - lastDampenRequestTime <= dampenTimeout;
        float rawDampen = requestIsFresh ? lastDampenValue : 0f;

        Vector3 towardSource = lastDampenSourcePosition - transform.position;
        towardSource.y = 0f;

        float directionFactor = 1f;
        if (velocity.sqrMagnitude > 0.01f && towardSource.sqrMagnitude > 0.01f)
        {
            directionFactor = Mathf.Clamp01(Vector3.Dot(velocity.normalized, towardSource.normalized));
        }

        float target = rawDampen * directionFactor;
        dampener = Mathf.MoveTowards(dampener, target, dampenDecaySpeed * Time.deltaTime);

        Vector2 input = movementAction.ReadValue<Vector2>();
        if (jumpAction.WasPressedThisFrame())
        {
            jumpQueued = true;
        }

        if (isGrinding)
        {
            HandleGrinding();
        }
        else
        {
            CheckGround();
            HandleTurning(input);

            if (isGrounded)
            {
                HandleGroundedMovement(input);
            }
            else
            {
                velocity.y += gravity * Time.deltaTime;
                if (reattachTimer > 0f) reattachTimer -= Time.deltaTime;

                if (grindAction.IsPressed())
                {
                    TryAttachToRail();
                }
            }

            controller.Move(velocity * Time.deltaTime);
        }

        jumpQueued = false;
        UpdateVisualTilt();
        UpdateCameraFOV();

        if (cameraShake != null)
        {
            cameraShake.isShaking = isGrinding;
        }
    }

    void HandleTurning(Vector2 input)
    {
        float speedFactor = Mathf.Clamp01(velocity.magnitude / maxSpeed);
        float turnFactor = Mathf.Lerp(minTurnSpeedFactor, 1f, speedFactor);
        float turnAmount = input.x * turnFactor;
        transform.Rotate(Vector3.up, turnAmount * turnSpeed * Time.deltaTime, Space.Self);

        lastTurnAmount = turnAmount;
    }

    void HandleGroundedMovement(Vector2 input)
    {
        Vector3 slopeForward = Vector3.ProjectOnPlane(transform.forward, groundNormal).normalized;

        velocity += slopeForward * (input.y * acceleration * Time.deltaTime);

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

        float effectiveMaxSpeed = Mathf.Lerp(maxSpeed, maxSpeed * dampenedSpeedFactor, dampener);
        Vector3 horizontal = Vector3.ClampMagnitude(new Vector3(velocity.x, 0f, velocity.z), effectiveMaxSpeed);
        velocity.x = horizontal.x;
        velocity.z = horizontal.z;

        if (jumpQueued) velocity.y += jumpForce;
    }

    public void RequestDampen(float amount, Vector3 sourcePosition)
    {
        float clamped = Mathf.Clamp01(amount);
        bool stillFresh = Time.time - lastDampenRequestTime <= dampenTimeout;
        
        if (!stillFresh || clamped >= lastDampenValue)
        {
            lastDampenValue = clamped;
            lastDampenSourcePosition = sourcePosition;
        }

        lastDampenRequestTime = Time.time;
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

        if (isGrinding)
        {
            targetLocalTilt = Quaternion.Euler(0f, 90f, 0f);
        }
        else if (isGrounded)
        {
            Vector3 localNormal = transform.InverseTransformDirection(groundNormal);
            targetLocalTilt = Quaternion.FromToRotation(Vector3.up, localNormal);
        }
        else
        {
            float pitchT = Mathf.Clamp(velocity.y / airPitchVelocityRange, -1f, 1f);
            targetLocalTilt = Quaternion.Euler(-pitchT * maxAirPitch, 0f, 0f);
        }

        if (!isGrinding)
        {
            Quaternion leanRotation = Quaternion.Euler(0f, 0f, -lastTurnAmount * maxTurnLean);
            targetLocalTilt *= leanRotation;
        }
        
        bool justLanded = isGrounded && !wasGrounded;
        visualMesh.localRotation = justLanded
            ? targetLocalTilt
            : Quaternion.Slerp(visualMesh.localRotation, targetLocalTilt, tiltSpeed * Time.deltaTime);

        wasGrounded = isGrounded;
    }

    void UpdateCameraFOV()
    {
        float speed = velocity.magnitude;
        float t = Mathf.Clamp01(speed / maxSpeed);
        float fovLerpAmt = fovCurve.Evaluate(t);
        float newFov = Mathf.Lerp(minFov, maxFov, fovLerpAmt);
        if (CameraController.Instance)
        {
            CameraController.Instance.SetPlayerFov(newFov);
        }
    }

    GrindRail FindNearbyRail(out Vector3 attachPoint, out float distanceAlongRail)
    {
        GrindRail closestRail = null;
        attachPoint = Vector3.zero;
        distanceAlongRail = 0f;
        float closestDistance = grindAttachRadius;

        foreach (GrindRail rail in GrindRail.All)
        {
            Vector3 point = rail.getClosestPoint(transform.position, out float dist);
            float distance = Vector3.Distance(transform.position, point);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestRail = rail;
                attachPoint = point;
                distanceAlongRail = dist;
            }
        }

        return closestRail;
    }

    void TryAttachToRail()
    {
        if (reattachTimer > 0f) { return; }

        GrindRail rail = FindNearbyRail(out Vector3 attachPoint, out float distanceAlongRail);
        if (rail == null) { return; }

        rail.getPointAtDistance(distanceAlongRail, out Vector3 tangent);
        float speedAlongRail = Vector3.Dot(velocity, tangent);
        grindSpeed = Mathf.Sign(speedAlongRail) * Mathf.Clamp(Mathf.Abs(speedAlongRail), minGrindSpeed, maxGrindSpeed);

        isGrinding = true;
        currentRail = rail;
        railDistance = distanceAlongRail;

        Vector3 facing = grindSpeed >= 0f ? tangent : -tangent;
        transform.rotation = Quaternion.LookRotation(facing, Vector3.up);
    }

    void HandleGrinding()
    {
        if (jumpQueued)
        {
            ExitGrind();
            velocity.y += jumpForce;
            return;
        }

        railDistance += grindSpeed * Time.deltaTime;

        if (!currentRail.isClosedLoop && (railDistance <= 0f || railDistance >= currentRail.length))
        {
            ExitGrind();
            return;
        }

        Vector3 targetPosition = currentRail.getPointAtDistance(railDistance, out Vector3 tangent);
        controller.Move(targetPosition - transform.position);
        Vector3 facing = grindSpeed >= 0f ? tangent : -tangent;
        Quaternion targetRotation = Quaternion.LookRotation(facing, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, grindTurnSpeed * Time.deltaTime);
    }

    void ExitGrind()
    {
        currentRail.getPointAtDistance(railDistance, out Vector3 tangent);
        velocity = tangent * grindSpeed;
        isGrinding = false;
        currentRail = null;
        reattachTimer = grindReattachDelay;
    }
}