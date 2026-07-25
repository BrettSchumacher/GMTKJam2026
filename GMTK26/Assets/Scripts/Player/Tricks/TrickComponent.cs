using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public enum InputDirection { DownLeft, Down, DownRight, Left, Neutral, Right, UpLeft, Up, UpRight }

public enum ButtonAction { Grab, Flip, Grind, Ollie, None }

public struct BufferedInput
{
    public InputDirection Direction;
    public ButtonAction Action;
    public float Time;
    public bool isDirection;

    public BufferedInput(InputDirection direction)
    {
        Direction = direction;
        Action = ButtonAction.None;
        Time = UnityEngine.Time.time;
        isDirection = true;
    }

    public BufferedInput(ButtonAction action)
    {
        Action = action;
        Direction = InputDirection.Neutral;
        Time = UnityEngine.Time.time;
        isDirection = false;
    }
    public bool IsDirection => isDirection;
    public bool IsAction =>  !isDirection;
}

public class TrickComponent : MonoBehaviour
{
    [ReadOnly] public List<TricksSO> TrickList = new();
    [SerializeField] private float inputBufferTime = 0.5f;
    [SerializeField] private float trickInputDelay = 0.08f;
    private bool pendingTrickCheck;

    private float lastInputTime;

    private readonly List<BufferedInput> inputBuffer = new();
    private InputDirection lastDirection = InputDirection.Neutral;

    private PlayerInput playerInput;
    private InputAction movementAction;
    private InputAction ollieAction;
    private InputAction grabAction;
    private InputAction flipAction;
    private InputAction grindAction;

    private void Awake()
    {
        LoadTricks();
    }

    private void OnEnable()
    {
        playerInput = GetComponent<PlayerInput>();
        movementAction = playerInput.actions["Movement"];
        ollieAction = playerInput.actions["Ollie"];
        grabAction = playerInput.actions["Grab"];
        flipAction = playerInput.actions["Flip"];
        grindAction = playerInput.actions["Grind"];

        ollieAction.performed += _ => PressAction(ButtonAction.Ollie);
        grabAction.performed += _ => PressAction(ButtonAction.Grab);
        flipAction.performed += _ => PressAction(ButtonAction.Flip);
        grindAction.performed += _ => PressAction(ButtonAction.Grind);
    }

    private void OnDisable()
    {
        ollieAction.performed -= _ => PressAction(ButtonAction.Ollie);
        grabAction.performed -= _ => PressAction(ButtonAction.Grab);
        flipAction.performed -= _ => PressAction(ButtonAction.Flip);
        grindAction.performed -= _ => PressAction(ButtonAction.Grind);
    }

    private void LoadTricks()
    {
        TrickList = Resources.LoadAll<TricksSO>("Tricks").Where(t => t != null).ToList();
        Debug.Log($"Loaded {TrickList.Count} tricks", this);

        foreach (TricksSO trick in TrickList)
        {
            Debug.Log($"Loaded Trick: {trick.TrickName}", trick);
        }
    }

    private void Update()
    {
        ReadDirection();
        CleanupBuffer();

        if (pendingTrickCheck && Time.time - lastInputTime >= trickInputDelay)
        {
            pendingTrickCheck = false;
            CheckTricks();
        }
    }

    private void ReadDirection()
    {
        Vector2 input = movementAction.ReadValue<Vector2>();
        InputDirection direction = ConvertDirection(input);

        if (direction != InputDirection.Neutral && direction != lastDirection)
        {
            inputBuffer.Add(new BufferedInput(direction));
        }

        lastDirection = direction;
    }

    public void PressAction(ButtonAction button)
    {
        if (button == ButtonAction.None)
            return;

        inputBuffer.Add(new BufferedInput(button));
        QueueTrickCheck();
    }

    private InputDirection ConvertDirection(Vector2 input)
    {
        if (input.magnitude < 0.2f)
            return InputDirection.Neutral;

        float angle = Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg;

        if (angle >= -22.5f && angle < 22.5f)
            return InputDirection.Right;

        if (angle >= 22.5f && angle < 67.5f)
            return InputDirection.UpRight;

        if (angle >= 67.5f && angle < 112.5f)
            return InputDirection.Up;

        if (angle >= 112.5f && angle < 157.5f)
            return InputDirection.UpLeft;

        if (angle >= 157.5f || angle < -157.5f)
            return InputDirection.Left;

        if (angle >= -157.5f && angle < -112.5f)
            return InputDirection.DownLeft;

        if (angle >= -112.5f && angle < -67.5f)
            return InputDirection.Down;

        return InputDirection.DownRight;
    }

    private void CleanupBuffer()
    {
        float cutoff = Time.time - inputBufferTime;
        inputBuffer.RemoveAll(x => x.Time < cutoff);
    }

    private void CheckTricks()
    {
        if (TrickList == null || TrickList.Count == 0)
            return;

        TricksSO best = null;

        foreach (TricksSO trick in TrickList)
        {
            if (Matches(trick))
            {
                if (best == null || Score(trick) > Score(best))
                {
                    best = trick;
                }
            }
        }

        if (best != null)
        {
            Debug.Log($"TRICK FOUND: {best.TrickName}", best);
            inputBuffer.Clear();
        }
    }

    private void QueueTrickCheck()
    {
        lastInputTime = Time.time;
        pendingTrickCheck = true;
    }

    private int Score(TricksSO trick)
    {
        int directions = trick.InputString.Count;
        int buttons = Mathf.Max(1, trick.ButtonPresses);

        // Total inputs is the primary measure.
        int totalInputs = directions + buttons;

        // Prefer more button presses if the total length is equal.
        return totalInputs * 100 + buttons;
    }


    private bool Matches(TricksSO trick)
    {
        if (trick == null)
            return false;

        int bufferIndex = inputBuffer.Count - 1;

        int requiredButtonPresses = Mathf.Max(1, trick.ButtonPresses);

        for (int press = 0; press < requiredButtonPresses; press++)
        {
            while (bufferIndex >= 0 && inputBuffer[bufferIndex].IsDirection)
                bufferIndex--;

            if (bufferIndex < 0)
                return false;

            if (!inputBuffer[bufferIndex].IsAction)
                return false;

            if (inputBuffer[bufferIndex].Action != trick.Button)
                return false;

            bufferIndex--;
        }

        if (trick.InputString != null)
        {
            for (int i = trick.InputString.Count - 1; i >= 0; i--)
            {
                while (bufferIndex >= 0 && inputBuffer[bufferIndex].IsAction)
                    bufferIndex--;

                if (bufferIndex < 0)
                    return false;

                if (inputBuffer[bufferIndex].Direction != trick.InputString[i])
                    return false;

                bufferIndex--;
            }
        }

        return true;
    }
}