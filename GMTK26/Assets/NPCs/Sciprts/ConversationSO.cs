using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Conversation", menuName = "ScriptableObjects/Conversation", order = 1)]
public class ConversationSO : ScriptableObject
{
    public TextAsset Conversation;
    public ItemsSO WeddingItem;
    public MusicTracks Music;
    public float MusicVolume;

    public ConversationData GetConversationData()
    {
        if (Conversation)
        {
            return DialogueHelpers.LoadConversationFromCsvString(Conversation.text);
        }

        return new ConversationData();
    }
}
