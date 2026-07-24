using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    private bool bHasActiveDialogue = false;

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
        return false;
    }
}
