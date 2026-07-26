using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcPlacement : MonoBehaviour
{
    public static List<NpcPlacement> NpcPlacements;

    public ConversationSO Conversation;

    private void Awake()
    {
        NpcPlacements ??= new();
        NpcPlacements.Add(this);
    }

    private void OnDestroy()
    {
        NpcPlacements.Remove(this);
    }

    public void Interact()
    {
        ConversationData convo = Conversation.GetConversationData();
        if (!convo.IsEmpty())
        {
            AudioManager.PlayBackgroundMusic(Conversation.Music, Conversation.MusicVolume);
            DialogueManager.Instance.StartDialogue(convo, OnDialogueCompleted);
        }
        else
        {
            OnDialogueCompleted();
        }
    }

    public void OnDialogueCompleted()
    {
        AudioManager.PlayBackgroundMusic(MusicTracks.Background, 0.5f);
        TradingSystem.Instance.TradeItem(Conversation.WeddingItem);
    }
}
