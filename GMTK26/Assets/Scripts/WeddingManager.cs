using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Cinemachine;
using UnityEngine;

public class WeddingManager : MonoBehaviour
{
    [Header("Conversations")]
    public TextAsset WeddingIntroConvo;
    public TextAsset WeddingMainConvo;
    
    public TextAsset NoWeddingItemsConvo;
    public TextAsset NotAllWeddingItemsConvo;
    public TextAsset AllWeddingItemsConvo;

    [Header("Visuals")] 
    public CinemachineVirtualCamera WeddingCamIntro;
    public CinemachineVirtualCamera WeddingCamVows;
    public CinemachineVirtualCamera WeddingCamEnd;
    
    [SerializedDictionary("Item", "Conversation")]
    public SerializedDictionary<ItemsSO, TextAsset> ItemToWeddingDialogue = new();

    private bool weddingStarted = false;
    private ConversationData FullWeddingConversation;
    private ConversationData WeddingVowsConversation;

    public void BeginWedding()
    {
        if (weddingStarted)
        {
            Debug.LogError("Already started wedding");
            return;
        }

        weddingStarted = true;
        
        AudioManager.PlayBackgroundMusic(MusicTracks.Wedding_Intro, 0.5f);
        // Maybe set player position

        if (InputManager.Instance)
        {
            InputManager.Instance.PushInputState(InputState.Cutscene);
        }
        
        SetIntroWeddingCamera();
    }

    private void SetIntroWeddingCamera()
    {
        if (CameraController.Instance && WeddingCamIntro)
        {
            CameraController.Instance.PushCamera(WeddingCamIntro);
        }
        
        // probably convert to coroutine and wait for anim to finish
        StartIntroWeddingDialogue();
    }

    private void StartIntroWeddingDialogue()
    {
        PopulateIntroConversation();

        if (!DialogueManager.Instance)
        {
            Debug.LogError("No dialogue manager found");
            OnIntroWeddingDialogueFinished();
        }
        else if (FullWeddingConversation.ConversationEntries.Count == 0)
        {
            Debug.LogError("No wedding conversation filled");
            OnIntroWeddingDialogueFinished();
        }
        else
        {
            DialogueManager.Instance.StartDialogue(FullWeddingConversation, OnIntroWeddingDialogueFinished);
        }
    }

    private void OnIntroWeddingDialogueFinished()
    {
        AudioManager.PlayBackgroundMusic(MusicTracks.Wedding_Vows, 0.5f);

        if (CameraController.Instance && WeddingCamVows)
        {
            CameraController.Instance.PushCamera(WeddingCamVows);
        }
        
        // maybe wait for camera transition
        PopulateVowsConversation();
        
        if (!DialogueManager.Instance)
        {
            Debug.LogError("No dialogue manager found");
            OnIntroWeddingDialogueFinished();
        }
        else if (WeddingVowsConversation.ConversationEntries.Count == 0)
        {
            Debug.LogError("No wedding conversation filled");
            OnIntroWeddingDialogueFinished();
        }
        else
        {
            DialogueManager.Instance.StartDialogue(WeddingVowsConversation, OnVowsDialogueFinished);
        }
    }

    private void OnVowsDialogueFinished()
    {
        if (CameraController.Instance && WeddingCamEnd)
        {
            CameraController.Instance.PushCamera(WeddingCamEnd);
        }
        
        // Do ending stuff here
    }

    private void PopulateIntroConversation()
    {
        FullWeddingConversation = new ConversationData();
        if (WeddingIntroConvo)
        {
            FullWeddingConversation.AppendConversation(DialogueHelpers.LoadConversationFromCsvString(WeddingIntroConvo.text));
        }

        if (!TradingSystem.Instance)
        {
            Debug.LogError("No trading system found");
        }

        List<ItemsSO> obtainedItems = TradingSystem.Instance
            ? TradingSystem.Instance.GetObtainedWeddingItems()
            : new List<ItemsSO>();

        if (obtainedItems.Count == 0 && NoWeddingItemsConvo)
        {
            FullWeddingConversation.AppendConversation(DialogueHelpers.LoadConversationFromCsvString(NoWeddingItemsConvo.text));
        }

        bool gotAllItems = true;
        // Iterate through conversations dict instead of obtained items to ensure proper ordering
        foreach (var item in ItemToWeddingDialogue.Keys)
        {
            if (!obtainedItems.Contains(item))
            {
                gotAllItems = false;
                continue;
            }

            if (ItemToWeddingDialogue[item])
            {
                FullWeddingConversation.AppendConversation(DialogueHelpers.LoadConversationFromCsvString(ItemToWeddingDialogue[item].text));
            }
        }

        if (gotAllItems && AllWeddingItemsConvo)
        {
            FullWeddingConversation.AppendConversation(DialogueHelpers.LoadConversationFromCsvString(AllWeddingItemsConvo.text));
        }
        else if (!gotAllItems && obtainedItems.Count > 0 && NotAllWeddingItemsConvo)
        {
            FullWeddingConversation.AppendConversation(DialogueHelpers.LoadConversationFromCsvString(NotAllWeddingItemsConvo.text));
        }
    }

    private void PopulateVowsConversation()
    {
        WeddingVowsConversation = new ConversationData();
        if (WeddingMainConvo)
        {
            WeddingVowsConversation.AppendConversation(DialogueHelpers.LoadConversationFromCsvString(WeddingMainConvo.text));
        }
    }
}
