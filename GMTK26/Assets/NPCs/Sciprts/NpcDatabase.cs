using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public struct NpcData
{
    public string Name;
    public Sprite Portrait;
}

[CreateAssetMenu(fileName = "NpcDatabase", menuName = "ScriptableObjects/NpcDatabase", order = 1)]
public class NpcDatabase : ScriptableObject
{
    public List<NpcData> Npcs;

    public Sprite GetNpcSprite(string name)
    {
        return Npcs.FirstOrDefault(npc => npc.Name == name).Portrait;
    }
}
