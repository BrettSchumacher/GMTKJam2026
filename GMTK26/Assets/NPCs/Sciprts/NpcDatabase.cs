using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct NpcData
{
    public string DisplayName;
    public string Id;
    public Sprite Portrait;
    public TextAsset[] Conversations;
}

[CreateAssetMenu(fileName = "NpcDatabase", menuName = "ScriptableObjects/NpcDatabase", order = 1)]
public class NpcDatabase : ScriptableObject
{
    public List<NpcData> Npcs;
}
