using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public enum InputDirection { DownLeft, Down, DownRight, Left, Neutral, Right, UpLeft, Up, UpRight }
public enum ButtonAction { Grab, Flip, Grind, Ollie, None }

public struct BufferedInput
{
    public InputDirection Direction;
    public ButtonAction Action;
    public float Time;
    public float lastTapTime;
    public bool isDirection;
    public int TapCount;

    public BufferedInput(InputDirection direction)
    {
        Direction = direction;
        Action = ButtonAction.None;
        Time = UnityEngine.Time.time;
        lastTapTime = Time;
        isDirection = true;
        TapCount = 1;
    }

    public BufferedInput(ButtonAction action)
    {
        Action = action;
        Direction = InputDirection.Neutral;
        Time = UnityEngine.Time.time;
        lastTapTime = Time;
        isDirection = false;
        TapCount = 1;
    }

    public bool IsDirection => isDirection;
    public bool IsAction => !isDirection;
}
public struct TrickMatch
{
    public TricksSO Trick;
    public int TapCount;
}

public class TrickComponent : MonoBehaviour
{
    //Change me if a trick seems like it might not be added to the list
    private List<TricksSO> TrickList = new();
    [SerializeField] private float inputBufferTime = 0.7f;
    [SerializeField] private float trickInputDelay = 0.25f;
    [SerializeField] private float trickInputTimeout = 0.9f;
    [SerializeField] private float maxButtonTapGap = 0.3f;
    [SerializeField] private float maxDirectionTapGap = 0.4f;

    private bool pendingTrickCheck;
    private float lastInputTime;
    private float firstInputTime;

    private List<BufferedInput> inputBuffer = new();
    private InputDirection lastDirection = InputDirection.Neutral;
    private bool wasAtNeutral = true;
    private TrickMatch lastTrick;

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
        //Debug.Log($"Loaded {TrickList.Count} tricks", this);
        //foreach (TricksSO trick in TrickList)
        //{
        //    Debug.Log($"Loaded Trick: {trick.TrickName}", trick);
        //}
    }

    private void Update()
    {
        ReadDirection();
        CleanupBuffer();

        if (pendingTrickCheck && ((Time.time - lastInputTime >= CurrentTrickCheckDelay()) || (Time.time - firstInputTime >= trickInputTimeout)))
        {
            pendingTrickCheck = false;
            firstInputTime = -10.0f;
            CheckTricks();
        }
    }


#region Direction Handling

    private void ReadDirection()
    {
        Vector2 input = movementAction.ReadValue<Vector2>();
        InputDirection direction = ConvertDirection(input);

        if (direction != InputDirection.Neutral)
        {
            if (direction != lastDirection)
            {
                // Only treat this as a "repeat direction tap" if the stick actually passed back through neutral
                bool cameFromNeutral = wasAtNeutral;
                bool hasLast = TryGetLastMatchingDirection(direction, out BufferedInput last);

                if (cameFromNeutral && hasLast && Time.time - last.Time <= maxButtonTapGap && last.TapCount < TrickLimits.MaxTrickButtonPress)
                {
                    IncrementLastDirection(direction);
                }
                else
                {
                    inputBuffer.Add(new BufferedInput(direction));
                }
            }
            else if (!RefreshHeldDirection(direction))
            {
                // Player is still holding the same direction as last frame, but its buffer entry is gone so add it back for trick spamming
                inputBuffer.Add(new BufferedInput(direction));
            }
        }

        wasAtNeutral = direction == InputDirection.Neutral;
        lastDirection = direction;
    }

    // Keeps a held direction's buffer entry getting cleaned up after a trick if the player is still holding it
    private bool RefreshHeldDirection(InputDirection direction)
    {
        for (int i = inputBuffer.Count - 1; i >= 0; i--)
        {
            if (inputBuffer[i].IsAction)
                continue;

            if (inputBuffer[i].Direction != direction)
                return false;

            BufferedInput updated = inputBuffer[i];
            updated.Time = Time.time;
            //Timeout multidirection taps
            if (updated.TapCount > 1 && Time.time - updated.lastTapTime > maxDirectionTapGap)
            {
                updated.lastTapTime = Time.time;
                updated.TapCount = Mathf.Max(updated.TapCount - 1, 1);
            }
            inputBuffer[i] = updated;
            return true;
        }

        return false;
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
    private void IncrementLastDirection(InputDirection direction)
    {
        for (int i = inputBuffer.Count - 1; i >= 0; i--)
        {
            if (inputBuffer[i].IsAction)
                continue;

            if (inputBuffer[i].Direction == direction)
            {
                BufferedInput updated = inputBuffer[i];
                updated.TapCount++;
                updated.Time = Time.time; // refresh so buffer cleanup/window logic stays sane
                inputBuffer[i] = updated;
            }
            return;
        }
    }

    private bool TryGetLastMatchingDirection(InputDirection direction, out BufferedInput match)
    {
        for (int i = inputBuffer.Count - 1; i >= 0; i--)
        {
            if (inputBuffer[i].IsAction)
                continue;

            match = inputBuffer[i];
            return match.Direction == direction;
        }

        match = default;
        return false;
    }

    #endregion

#region Action/Button Handling
    public void PressAction(ButtonAction button)
    {
        if (button == ButtonAction.None)
            return;

        if (!pendingTrickCheck)
        {
            firstInputTime = Time.time;
        }

        if (TryGetLastMatchingAction(button, out BufferedInput last) && Time.time - last.Time <= maxButtonTapGap)
        {
            IncrementLastAction(button);
        }
        else
        {
            inputBuffer.Add(new BufferedInput(button));
        }

        QueueTrickCheck();
    }

    private bool TryGetLastMatchingAction(ButtonAction button, out BufferedInput match)
    {
        for (int i = inputBuffer.Count - 1; i >= 0; i--)
        {
            if (inputBuffer[i].IsDirection)
                continue;

            match = inputBuffer[i];
            return match.Action == button;
        }

        match = default;
        return false;
    }

    private void IncrementLastAction(ButtonAction button)
    {
        for (int i = inputBuffer.Count - 1; i >= 0; i--)
        {
            if (inputBuffer[i].IsDirection)
                continue;

            if (inputBuffer[i].Action == button)
            {
                BufferedInput updated = inputBuffer[i];
                updated.TapCount++;
                //Debug.Log($"Incremented to {updated.TapCount}");
                updated.Time = Time.time;
                inputBuffer[i] = updated;
            }
            return;
        }
    }
    #endregion

    #region Timing Shit
    // Multipress inputs have a slightly more lenient window between input press and trick registering. This is also reset on each press (unless trickInputTimeout is reached)
    private float CurrentTrickCheckDelay()
    {
        for (int i = inputBuffer.Count - 1; i >= 0; i--)
        {
            if (inputBuffer[i].IsDirection)
                continue;

            BufferedInput entry = inputBuffer[i];
            if (entry.TapCount < TrickLimits.MaxTrickButtonPress)
            {
                foreach (TricksSO trick in TrickList)
                {
                    if (trick != null && trick.AllowMultiTap && trick.Button == entry.Action)
                        return maxButtonTapGap;
                }
            }

            break;
        }

        return trickInputDelay;
    }

    private void CleanupBuffer()
    {
        float cutoff = Time.time - inputBufferTime;
        inputBuffer.RemoveAll(x => x.Time < cutoff);
    }
    private void QueueTrickCheck()
    {
        lastInputTime = Time.time;
        pendingTrickCheck = true;
    }
#endregion


    private void CheckTricks()
    {
        if (TrickList == null || TrickList.Count == 0)
            return;

        TrickMatch best = default;
        bool found = false;

        foreach (TricksSO trick in TrickList)
        {
            if (TryMatch(trick, out TrickMatch match))
            {
                if (!found || Score(match) > Score(best))
                {
                    best = match;
                    found = true;
                }

                //Edge case of matching inputs tieing
                if (found && Score(match) == Score(best))
                {
                    //If you did that trick last time it will technically be scored slightly less so pick the other match
                    if (lastTrick.Trick != null && best.Trick.name == lastTrick.Trick.name)
                    {
                        best = match;
                        found = true;
                    }

                    // Fuck it two things are tied for value so just coin flip which one to do
                    else if (UnityEngine.Random.Range(0,2) == 1)
                    {
                        best = match;
                        found = true;
                    }
                }
            }
        }

        if (found)
        {
            Debug.Log($"Did a {best.Trick.GetDisplayName(best.TapCount)} (AnimID: {best.Trick.GetAnimationID(best.TapCount)}, Points: {best.Trick.GetPointValue(best.TapCount)}");
            lastTrick = best;
            inputBuffer.Clear();

            // If the player is still holding a direction when the trick fires, keep it "live" in the buffer in case they are trying to multitap
            if (lastDirection != InputDirection.Neutral)
            {
                inputBuffer.Add(new BufferedInput(lastDirection));
            }
        }
    }



    // If we have multiple potential matches, try and usually just get whatever is the highest point value but sometimes more likely based on matching inputs  to tiebreak 
    // and asssume that was the one the player wanted.
    // This could probably be reworked to like real "scoring" of the inputbuffer to get more accurately guesses on what the player was trying to do but like.... eh?
    private int Score(TrickMatch match)
    {
        int matchingDir = match.Trick.InputString?.Count ?? 0;
        int baseInputScore = matchingDir + match.TapCount;

        if (match.TapCount <= 1 || !match.Trick.AllowMultiTap)
        {
            return match.Trick.PointValue + baseInputScore;
        }

        int index = match.Trick.multiTapOverrides.Length - 1;
        if (match.TapCount < index)
        {
            index = match.TapCount - 1;
        }
        return match.Trick.multiTapOverrides[index].PointOverride + baseInputScore;
    }

    // ngl, I wrote this and I genuinely still don't really understand how this works. I think I am just fucking stupid or have braindamage or something - Boschy
    private bool TryMatch(TricksSO trick, out TrickMatch match)
    {
        match = default;

        if (trick == null)
            return false;

        int bufferIndex = inputBuffer.Count - 1;
        while (bufferIndex >= 0 && inputBuffer[bufferIndex].IsDirection)
            bufferIndex--;


        if (bufferIndex < 0)
            return false;

        BufferedInput buttonEntry = inputBuffer[bufferIndex];

        if (!buttonEntry.IsAction || buttonEntry.Action != trick.Button)
            return false;

        int tapCount = Mathf.Clamp(buttonEntry.TapCount, 1, trick.maxTaps);
        bufferIndex--;

        if (trick.InputString != null)
        {
            int i = trick.InputString.Count - 1;
            while (i >= 0)
            {
                InputDirection requiredDir = trick.InputString[i];
                int requiredRun = 1;
                while (i - requiredRun >= 0 && trick.InputString[i - requiredRun] == requiredDir)
                    requiredRun++;

                while (bufferIndex >= 0 && inputBuffer[bufferIndex].IsAction)
                    bufferIndex--;

                if (bufferIndex < 0)
                    return false;

                BufferedInput dirEntry = inputBuffer[bufferIndex];

                if (dirEntry.Direction != requiredDir || dirEntry.TapCount < requiredRun)
                    return false;

                bufferIndex--;
                i -= requiredRun;
            }
        }

        match = new TrickMatch { Trick = trick, TapCount = tapCount };
        return true;
    }
}