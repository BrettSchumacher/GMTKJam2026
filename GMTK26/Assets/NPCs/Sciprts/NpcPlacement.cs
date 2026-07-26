using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcPlacement : MonoBehaviour
{
    public static Dictionary<string, List<NpcPlacement>> NpcPlacementsById;
    
    public string NpcId;
    public int ConversationIndex = 0;

    private void Awake()
    {
        NpcPlacementsById ??= new();

        NpcPlacementsById[NpcId] ??= new();
        NpcPlacementsById[NpcId].Add(this);
    }
}
