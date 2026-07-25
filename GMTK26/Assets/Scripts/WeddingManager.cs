using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

public class WeddingManager : MonoBehaviour
{
    public TextAsset WeddingIntroConvo;
    public TextAsset WeddingMainConvo;
    
    public TextAsset NoWeddingItemsConvo;
    public TextAsset NotAllWeddingItemsConvo;
    public TextAsset AllWeddingItemsConvo;
    
    [SerializedDictionary("Item", "Conversation")]
    public SerializedDictionary<ItemsSO, TextAsset> ItemToWeddingDialogue = new();

    private bool weddingStarted = false;
    private ConversationData FullWeddingConversation;

    public void BeginWedding()
    {
        if (weddingStarted)
        {
            Debug.LogError("Already started wedding");
            return;
        }

        weddingStarted = true;
        
        // Adjust input to stop player movement
        // Maybe set player position
        
        SetWeddingCamera();
    }

    private void SetWeddingCamera()
    {
        // do camera stuff
        // start wedding audio
        // probably convert to coroutine and wait for anim to finish
        StartWeddingDialogue();
    }

    private void StartWeddingDialogue()
    {
        PopulateConversation();

        if (!DialogueManager.Instance)
        {
            Debug.LogError("No dialogue manager found");
            OnWeddingDialogueFinished();
        }
        else if (FullWeddingConversation.ConversationEntries.Count == 0)
        {
            Debug.LogError("No wedding conversation filled");
            OnWeddingDialogueFinished();
        }
        else
        {
            DialogueManager.Instance.StartDialogue(FullWeddingConversation, OnWeddingDialogueFinished);
        }
    }

    private void OnWeddingDialogueFinished()
    {
        // maybe do final music change
        // final camera anim
        // go to game over
    }

    private void PopulateConversation()
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

        if (WeddingMainConvo)
        {
            FullWeddingConversation.AppendConversation(DialogueHelpers.LoadConversationFromCsvString(WeddingMainConvo.text));
        }
    }
}
