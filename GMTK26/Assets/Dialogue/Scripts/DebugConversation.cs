using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugConversation : MonoBehaviour
{
    public TextAsset ConversationFile;
    
    // Start is called before the first frame update
    void Start()
    {
        if (ConversationFile == null)
        {
            Debug.LogError("no conversation provided");
            return;
        }
        
        ConversationData conversationData = DialogueHelpers.LoadConversationFromCsvString(ConversationFile.text);
        if (conversationData == null || conversationData.IsEmpty())
        {
            Debug.LogError("empty or invalid conversation provided");
            return;
        }

        DialogueManager.Instance?.StartDialogue(conversationData, () => Debug.Log("Done!"));
    }
    
}
