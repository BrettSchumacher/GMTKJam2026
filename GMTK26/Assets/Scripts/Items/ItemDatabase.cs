using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "ScriptableObjects/ItemDatabase", order = 3)]
public class ItemDatabase : ScriptableObject
{
    public List<ItemsSO> items;
}
