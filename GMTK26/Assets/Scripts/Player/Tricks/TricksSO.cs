using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "NewTrick", menuName = "ScriptableObjects/Tricks", order = 1)]
public class TricksSO : ScriptableObject
{
    [SerializeField] public string TrickName;
    public List<InputDirection> InputString = new();
    public ButtonAction Button = ButtonAction.None;
    [Range(1,3)] public int ButtonPresses = 1;
    [SerializeField] public string AnimationID;
    [SerializeField] public int PointValue = 100;
    [SerializeField] public bool Holdable = false;
    [SerializeField] public bool SecretTrick = false;

    // Set the trickname field to the file name
#if UNITY_EDITOR
    private string lastAssetName;
    private void OnValidate()
    {
        string assetName = name;

        // Prevent infinite validation loop
        if (lastAssetName == assetName)
            return;

        lastAssetName = assetName;

        // Update field when asset filename changes
        if (TrickName != assetName)
        {
            TrickName = assetName;
            EditorUtility.SetDirty(this);
        }
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(TricksSO))]
public class TricksSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        TricksSO trick = (TricksSO)target;
        GUILayout.Space(20);
        if (GUILayout.Button("Rename Asset Name to Trick Name"))
        {
            AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(trick), trick.TrickName);
            AssetDatabase.SaveAssets();
        }
    }
}
#endif