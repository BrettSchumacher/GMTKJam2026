using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "NewItem", menuName = "ScriptableObjects/Items", order = 2)]
public class ItemsSO : ScriptableObject
{
    [SerializeField] public string ItemName;
    [SerializeField] public float Value;
    [SerializeField] public Image Image;
    [SerializeField] public bool forWedding;
}
