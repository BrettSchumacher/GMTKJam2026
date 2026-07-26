using System;
using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.InputSystem;

public enum InputState
{
    Game,
    Dialogue,
    Cutscene,
    Menu
}

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    public InputState DefaultInputState;
    [SerializedDictionary("Input State", "Action Map Name")]
    public SerializedDictionary<InputState, string> InputStateToActionMapName = new();

    private Stack<InputState> InputStates = new Stack<InputState>();
    private PlayerInput PlayerInputComponent;

    private void Awake()
    {
        if (Instance)
        {
            Debug.LogError("Duplicate InputManagers detected");
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (!PlayerManager.Instance)
        {
            Debug.LogError("No Player manager found");
            return;
        }

        PlayerInputComponent = PlayerManager.Instance.InputComponent;
        PushInputState(DefaultInputState);
    }

    public void PushInputState(InputState newState, bool clearStack = false)
    {
        if (!InputStateToActionMapName.ContainsKey(newState))
        {
            Debug.LogError("No action mapping found for state: " + newState);
            return;
        }

        if (InputStates.Count > 0 && newState == InputStates.Peek())
        {
            return;
        }

        if (InputStates.Count > 0)
        {
            Debug.Log("Going from input state " + InputStates.Peek() + " to " + newState);
        }
        else
        {
            Debug.Log("Setting input state to " + newState);
        }
        
        if (clearStack)
        {
            InputStates.Clear();
        }
        
        InputStates.Push(newState);
        SetInputState(newState);
    }

    public void PopInputState()
    {
        if (InputStates.Count == 0)
        {
            return;
        }

        InputStates.Pop();
        
        InputState newState =  InputStates.Count == 0 ? DefaultInputState : InputStates.Peek();
        SetInputState(newState);
    }

    private void SetInputState(InputState newState)
    {
        PlayerInputComponent.SwitchCurrentActionMap(InputStateToActionMapName[newState]);
    }
}
