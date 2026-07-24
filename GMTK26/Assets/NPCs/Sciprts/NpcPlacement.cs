using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NpcPlacementType
{
    INVALID,
    INITIAL, // Placement player encounters them first in main game area
    FINAL // Placement for final skatepark scene.
}

public class NpcPlacement : MonoBehaviour
{
    public static Dictionary<string, List<NpcPlacement>> NpcPlacementsById;
    
    public string NpcId;
    public int ConversationIndex = 0;
    public NpcPlacementType PlacementType;

    private void Awake()
    {
        NpcPlacementsById ??= new();

        NpcPlacementsById[NpcId] ??= new();
        NpcPlacementsById[NpcId].Add(this);
    }
}
