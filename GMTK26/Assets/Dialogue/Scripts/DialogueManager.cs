using System;
using System.Collections;
using System.Linq;
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
    LoadingPlayerText,
    ShowingPlayerText,
    Closing
}

[System.Serializable]
public struct PlayerChoiceFields
{
    public GameObject Root;
    public Image SelectedIcon;
    public TypewriterEffect TextBox;
    public Button Button;

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

    public NpcDatabase NpcDatabase;
    public GameObject NpcPortrait;
    public Image NpcPortraitImage;
    public TMP_Text NpcName;
    public GameObject PlayerPortrait;
    public Image PlayerPortraitImage;
    public TMP_Text PlayerName;
    public GameObject NpcTextRoot;
    public TypewriterEffect NpcTextBox;
    public GameObject PlayerText;
    public TypewriterEffect PlayerTextBox;
    public GameObject PlayerChoices;
    [FormerlySerializedAs("PlayerChoiceArrray")] public PlayerChoiceFields[] PlayerChoiceArray;

    private DialogueUIState currentState = DialogueUIState.Disabled;
    private ConversationData currentConversation;
    private int currentConversationEntryIndex;
    private Action currentConversationCompleteCallback;
    private int currentSelectedChoiceIndex = 0;

    void Awake()
    {
        if (Instance)
        {
            Debug.LogError("DialogueManager::Awake - Dialogue manager instance already exists");
            return;
        }

        Instance = this;
    }

    public bool StartDialogue(ConversationData conversation, Action callback)
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
        PlayerText.SetActive(isPlayer && !isPlayerChoice);
        PlayerChoices.SetActive(isPlayerChoice);

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
                PlayerChoiceArray[i].SelectedIcon.color = isSelectedChoice ? Color.white : Color.clear;
                PlayerChoiceArray[i].Button.onClick.AddListener(AdvanceConversation);
            }
        }
    }

    private void OpenDialogueUI()
    {
        currentState = DialogueUIState.Opening;
        
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
            TypewriterEffect activeTextBox = currentEntry.IsPlayer ? PlayerTextBox : NpcTextBox;
            currentState = currentEntry.IsPlayer ? DialogueUIState.LoadingPlayerText : DialogueUIState.LoadingNpcText;
            activeTextBox.NewText(currentEntry.Entries[0], () =>
            {
                currentState = currentEntry.IsPlayer ? DialogueUIState.ShowingPlayerText : DialogueUIState.ShowingNpcText;
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
        // only poll input if we're waiting for user to advance from showing text
        if (currentState != DialogueUIState.ShowingPlayerText && currentState != DialogueUIState.ShowingNpcText)
        {
            return;
        }

        if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame) // ||
            // Gamepad.current?.buttonSouth.wasPressedThisFrame)
        {
            AdvanceConversation();
        }
    }

    static DialogueUIState[] advancableStates =
    {
        DialogueUIState.ShowingNpcText,
        DialogueUIState.ShowingPlayerChoices,
        DialogueUIState.ShowingPlayerText
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
        
        for (int i = 0; i < PlayerChoiceArray.Length; ++i)
        {
            PlayerChoiceArray[i].Button.onClick.RemoveListener(AdvanceConversation);
        }
        
        // do some anim
        
        currentState = DialogueUIState.Disabled;
    
    }
}
