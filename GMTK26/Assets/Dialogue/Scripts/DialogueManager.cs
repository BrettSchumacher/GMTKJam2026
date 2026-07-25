using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

public enum DialogueUIState
{
    Disabled,
    Opening,
    Delaying,
    LoadingNpcText,
    ShowingNpcText,
    LoadingPlayerChoices,
    ShowingPlayerChoices,
    Closing
}

[System.Serializable]
public struct PlayerChoiceFields
{
    public GameObject Root;
    public Image[] SelectedIcons;
    public TypewriterEffect TextBox;

    private bool TextLoaded;

    public bool IsLoaded()
    {
        return TextLoaded;
    }

    public void SetTextLoaded(bool loaded)
    {
        TextLoaded = loaded;
    }
}

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    public Canvas DialogueCanvas;
    public NpcDatabase NpcDatabase;
    public GameObject NpcPortrait;
    public Image NpcPortraitImage;
    public TMP_Text NpcName;
    public GameObject PlayerPortrait;
    public Image PlayerPortraitImage;
    public TMP_Text PlayerName;
    public GameObject NpcTextRoot;
    public TypewriterEffect NpcTextBox;
    public GameObject PlayerChoices;
    [FormerlySerializedAs("PlayerChoiceArrray")] public PlayerChoiceFields[] PlayerChoiceArray;
    public bool UseDebugInputs = false;

    private DialogueUIState currentState = DialogueUIState.Disabled;
    private ConversationData currentConversation;
    private int currentConversationEntryIndex;
    private Action currentConversationCompleteCallback;
    private int currentSelectedChoiceIndex = 0;

    private PlayerInput inputComponent;
    private InputAction advanceInputAction;
    private InputAction upChoiceInputAction;
    private InputAction downChoiceInputAction;

    void Awake()
    {
        if (Instance)
        {
            Debug.LogError("DialogueManager::Awake - Dialogue manager instance already exists");
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        TryConfigureInput();
    }
    
    private void OnDisable()
    {
        CleanupInputCallbacks();
    }

    void TryConfigureInput()
    {
        if (!PlayerManager.Instance)
        {
            return;
        }

        inputComponent = PlayerManager.Instance.InputComponent;

        if (!inputComponent)
        {
            return;
        }

        advanceInputAction = inputComponent.actions.FindAction("AdvanceDialogue");
        upChoiceInputAction = inputComponent.actions.FindAction("DialogueChoiceUp");
        downChoiceInputAction = inputComponent.actions.FindAction("DialogueChoiceDown");

        if (advanceInputAction != null)
        {
            advanceInputAction.performed += OnAdvanceInput;
        }

        if (upChoiceInputAction != null)
        {
            upChoiceInputAction.performed += OnChoiceUpInput;
        }
        
        if (downChoiceInputAction != null)
        {
            downChoiceInputAction.performed += OnChoiceDownInput;
        }
    }

    void CleanupInputCallbacks()
    {
        if (advanceInputAction!= null)
        {
            advanceInputAction.performed -= OnAdvanceInput;
        }
        
        if (upChoiceInputAction != null)
        {
            upChoiceInputAction.performed -= OnChoiceUpInput;
        }
        
        if (downChoiceInputAction != null)
        {
            downChoiceInputAction.performed -= OnChoiceDownInput;
        }
    }

    public bool StartDialogue(ConversationData conversation, Action callback = null)
    {
        if (conversation == null)
        {
            Debug.LogError("DialogueManager::StartDialogue - Cannot start dialogue for null conversation");
            return false;
        }
        
        if (currentState != DialogueUIState.Disabled)
        {
            Debug.LogError("DialogueManager::StartDialogue - Tried starting conversation while one is already active");
            return false;
        }

        if (!conversation.ConversationEntries.Any())
        {
            Debug.LogError("DialogueManager::StartDialogue - Tried starting conversation with empty conversation");
            return false;
        }

        currentConversation = conversation;
        currentConversationEntryIndex = 0;
        currentSelectedChoiceIndex = 0;
        currentConversationCompleteCallback = callback;
        
        ConfigureUIForCurrentEntry();
        OpenDialogueUI();
        return true;
    }

    private void ConfigureUIForCurrentEntry()
    {
        if (currentConversation == null)
        {
            Debug.LogError("DialogueManager::ConfigureUIForCurrentEntry - Cannot configure UI for null conversation");
            return;
        }
        
        if (currentConversationEntryIndex < 0 ||
            currentConversationEntryIndex >= currentConversation.ConversationEntries.Count)
        {
            Debug.LogError("DialogueManager::ConfigureUIForCurrentEntry - Current entry index out of range: " + currentConversationEntryIndex);
            return;
        }

        ConversationEntry currentEntry = currentConversation.ConversationEntries[currentConversationEntryIndex];

        bool isPlayer = currentEntry.IsPlayer;
        bool isPlayerChoice = currentEntry.IsPlayerChoice();
        
        // Maybe do anims here instead
        NpcPortrait.SetActive(!isPlayer);
        PlayerPortrait.SetActive(isPlayer);
        NpcTextRoot.SetActive(!isPlayer);
        PlayerChoices.SetActive(isPlayer);

        List<TypewriterEffect> textsToReset = new(5) { NpcTextBox };
        textsToReset.AddRange(PlayerChoiceArray.Select(choice => choice.TextBox));

        foreach (var typewriter in textsToReset)
        {
            TMP_Text textbox = typewriter.GetComponent<TMP_Text>();
            if (textbox)
            {
                textbox.text = "";
            }
        }

        Sprite sprite = NpcDatabase.GetNpcSprite(currentEntry.CharacterName);

        if (isPlayer)
        {
            PlayerName?.SetText(currentEntry.CharacterName);
            PlayerPortraitImage.sprite = sprite;
        }
        else
        {
            NpcName?.SetText(currentEntry.CharacterName);
            NpcPortraitImage.sprite = sprite;
        }

        if (isPlayerChoice)
        {
            int numChoices = currentEntry.Entries.Length;
            if (numChoices > PlayerChoiceArray.Length)
            {
                Debug.LogError("DialogueManager::ConfigureUIForCurrentEntry - Too few UI choice objects for amount of dialogue options");
                numChoices = PlayerChoiceArray.Length;
            }

            currentSelectedChoiceIndex = Mathf.Clamp(currentSelectedChoiceIndex, 0, numChoices - 1);

            for (int i = 0; i < PlayerChoiceArray.Length; ++i)
            {
                bool enableChoice = numChoices > i;
                bool isSelectedChoice = currentSelectedChoiceIndex == i;
                
                PlayerChoiceArray[i].Root.SetActive(enableChoice);
                PlayerChoiceArray[i].SetTextLoaded(false);

                foreach (var icon in PlayerChoiceArray[i].SelectedIcons)
                {
                    icon.color = isSelectedChoice ? Color.white : Color.clear;
                }
            }
        }
    }

    private void OpenDialogueUI()
    {
        currentState = DialogueUIState.Opening;
        InputManager.Instance?.PushInputState(InputState.Dialogue);

        if (DialogueCanvas)
        {
            DialogueCanvas.enabled = true;
        }
        
        // Probably start coroutine here

        StartCoroutine(DelayedLoadText());
    }

    private IEnumerator DelayedLoadText()
    {
        currentState = DialogueUIState.Delaying;
        // wait one frame
        yield return 0;
        LoadText();
    }

    private void LoadText()
    {
        if (currentConversation == null)
        {
            Debug.LogError("DialogueManager::LoadText - Cannot load null conversation");
            return;
        }
        
        if (currentConversationEntryIndex < 0 ||
            currentConversationEntryIndex >= currentConversation.ConversationEntries.Count)
        {
            Debug.LogError("DialogueManager::LoadText - Current entry index out of range: " + currentConversationEntryIndex);
            return;
        }
        
        ConversationEntry currentEntry = currentConversation.ConversationEntries[currentConversationEntryIndex];

        if (!currentEntry.IsPlayerChoice())
        {
            currentState =  DialogueUIState.LoadingNpcText;
            NpcTextBox.NewText(currentEntry.Entries[0], () =>
            {
                currentState =  DialogueUIState.ShowingNpcText;
            });
            return;
        }

        currentState = DialogueUIState.LoadingPlayerChoices;
        
        int numChoices = Mathf.Min(currentEntry.Entries.Length, PlayerChoiceArray.Length);
        for (int i = 0; i < numChoices; ++i)
        {
            string choiceText = currentEntry.Entries[i];
            PlayerChoiceArray[i].SetTextLoaded(false);
            int index = i; // need a copy to pass into the lambda scope
            PlayerChoiceArray[i].TextBox.NewText(choiceText, () =>
            {
                PlayerChoiceArray[index].SetTextLoaded(true);
                CheckIfAllChoicesLoaded();
            });
        }
    }

    private void CheckIfAllChoicesLoaded()
    {
        ConversationEntry currentEntry = currentConversation.ConversationEntries[currentConversationEntryIndex];
        int numChoices = Mathf.Min(currentEntry.Entries.Length, PlayerChoiceArray.Length);
        for (int i = 0; i < numChoices; ++i)
        {
            if (!PlayerChoiceArray[i].IsLoaded())
            {
                return;
            }
        }

        currentState = DialogueUIState.ShowingPlayerChoices;
    }
    
    private void Update()
    {
        if (!UseDebugInputs)
        {
            return;
        }
        
        CheckForDebugAdvanceInput();
        CheckForDebugChoiceChangedInput();
    }
    
    void CheckForDebugAdvanceInput()
    {
        if (Keyboard.current.enterKey.wasPressedThisFrame || 
            Keyboard.current.spaceKey.wasPressedThisFrame || 
            Mouse.current.leftButton.wasPressedThisFrame) // || Gamepad.current.buttonSouth.wasPressedThisFrame)
        {
            OnAdvanceInput();
        }
    }
    
    void CheckForDebugChoiceChangedInput()
    {
        if (Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            TrySetChoiceSelected(currentSelectedChoiceIndex - 1);
            return;
        }

        if (Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            TrySetChoiceSelected(currentSelectedChoiceIndex + 1);
        }
    }

    // On advance input either finish loading current text or move the conversation onwards
    public void OnAdvanceInput(InputAction.CallbackContext context = default)
    {
        switch (currentState)
        {
            case DialogueUIState.ShowingNpcText: // fall through
            case DialogueUIState.ShowingPlayerChoices:
                AdvanceConversation();
                break;
            case DialogueUIState.LoadingPlayerChoices: // fall through
            case DialogueUIState.LoadingNpcText: // fall through
                SkipText();
                break;
        }
    }

    void OnChoiceUpInput(InputAction.CallbackContext context = default)
    {
        TrySetChoiceSelected(currentSelectedChoiceIndex - 1);
    }
    
    void OnChoiceDownInput(InputAction.CallbackContext context = default)
    {
        TrySetChoiceSelected(currentSelectedChoiceIndex + 1);
    }

    void SkipText()
    {
        List<TypewriterEffect> textsToSkip = new();
        switch (currentState)
        {
            case DialogueUIState.LoadingNpcText:
                textsToSkip.Add(NpcTextBox);
                break;
            case DialogueUIState.LoadingPlayerChoices:
                foreach (var playerChoice in PlayerChoiceArray)
                {
                    textsToSkip.Add(playerChoice.TextBox);
                }
                break;
        }

        foreach (var textToSkip in textsToSkip)
        {
            if (!textToSkip.isActiveAndEnabled)
            {
                continue;
            }
            textToSkip.SkipText();
        }
    }

    public void TrySetChoiceSelected(int index)
    {
        if (currentState != DialogueUIState.ShowingPlayerChoices &&
            currentState != DialogueUIState.LoadingPlayerChoices)
        {
            return;
        }

        int numChoices = currentConversation.ConversationEntries[currentConversationEntryIndex].Entries.Length;
        if (index < 0 || index >= numChoices)
        {
            return;
        }

        currentSelectedChoiceIndex = index;
        for (int i = 0; i < PlayerChoiceArray.Length; ++i)
        {
            bool isSelectedChoice = currentSelectedChoiceIndex == i;
            foreach (var icon in PlayerChoiceArray[i].SelectedIcons)
            {
                icon.color = isSelectedChoice ? Color.white : Color.clear;
            }
        }
    }

    static DialogueUIState[] advancableStates =
    {
        DialogueUIState.ShowingNpcText,
        DialogueUIState.ShowingPlayerChoices,
    };
    
    private void AdvanceConversation()
    {
        if (currentConversation == null)
        {
            Debug.LogError("DialogueManager::AdvanceConversation - Cannot advance null conversation");
            return;
        }

        if (!advancableStates.Contains(currentState))
        {
            Debug.Log("DialogueManager::AdvanceConversation - Cannot advance from current state, skipping: " + currentState);
            return;
        }
        
        currentConversationEntryIndex++;
        if (currentConversationEntryIndex >= currentConversation.ConversationEntries.Count)
        {
            CloseDialogeUI();
            return;
        }
        
        ConfigureUIForCurrentEntry();
        StartCoroutine(DelayedLoadText());
    }

    private void CloseDialogeUI()
    {
        currentState = DialogueUIState.Closing;
        
        Debug.Log("Conversation Finished");
        
        currentConversationCompleteCallback?.Invoke();

        InputManager.Instance?.PopInputState();
        
        // do some anim

        if (DialogueCanvas)
        {
            DialogueCanvas.enabled = false;
        }
        
        currentState = DialogueUIState.Disabled;
    
    }
}
