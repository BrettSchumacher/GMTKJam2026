using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TrickLimits
{
    public const int MaxTrickButtonPress = 3;
    public const int MaxTrickDirectionPress = 2;
    public const int MaxTrickRepeatReduction = 3;
    public static string GetCountPrefix(int tapCount)
    {
        return tapCount switch
        {
            2 => "Double",
            3 => "Triple",
            4 => "Quadruple",
            5 => "Quintuple",
            6 => "Sextuple",
            7 => "Septuple",
            8 => "Octuple",
            9 => "Nonuple",
            10 => "Decuple",
            _ => $"{tapCount}x"
        };
    }

    public static int GetPointMult(int tapCount)
    {
        return tapCount;
    }

}

[Serializable] public struct MultiTapOverride
{
    public string NameOverride;
    public int PointOverride;
}
[CreateAssetMenu(fileName = "NewTrick", menuName = "ScriptableObjects/Tricks", order = 1)]
public class TricksSO : ScriptableObject
{
    [SerializeField] public string TrickName;
    public List<InputDirection> InputString = new();
    public ButtonAction Button = ButtonAction.None;

    [SerializeField] public string AnimationID;
    [SerializeField] public int PointValue = 100;
    [SerializeField] public bool Holdable = false;
    [SerializeField] public bool SecretTrick = false;

    // Hides Multitap stuff if false
    [SerializeField] public bool AllowMultiTap = false; 
    [SerializeField] public MultiTapOverride[] multiTapOverrides;
    [SerializeField, Range(1,TrickLimits.MaxTrickRepeatReduction)] public int maxTaps = 3;



    public string GetDisplayName(int resolvedTapCount)
    {
        if (!AllowMultiTap || resolvedTapCount <= 1)
            return TrickName;

        if (TryGetOverride(resolvedTapCount, out MultiTapOverride overrideData) && !string.IsNullOrWhiteSpace(overrideData.NameOverride))
        {
            return overrideData.NameOverride;
        }

        return $"{TrickLimits.GetCountPrefix(resolvedTapCount)} {TrickName}";
    }

    public string GetAnimationID(int resolvedTapCount)
    {
        if (!AllowMultiTap || resolvedTapCount <= 1)
            return AnimationID;

        return $"{AnimationID}_{TrickLimits.GetCountPrefix(resolvedTapCount)}";
    }

    public int GetPointValue(int resolvedTapCount)
    {
        if (!AllowMultiTap || resolvedTapCount <= 1)
            return PointValue;

        if (TryGetOverride(resolvedTapCount, out MultiTapOverride overrideData) && overrideData.PointOverride > 0)
            return overrideData.PointOverride;

        return PointValue * TrickLimits.GetPointMult(resolvedTapCount);
    }


    private bool TryGetOverride(int resolvedTapCount, out MultiTapOverride overrideData)
    {
        overrideData = default;

        int index = resolvedTapCount - 2;
        if (multiTapOverrides == null || index < 0 || index >= multiTapOverrides.Length)
            return false;

        overrideData = multiTapOverrides[index];
        return true;
    }

#if UNITY_EDITOR
    [SerializeField, HideInInspector] private string lastAssetName;
    [SerializeField, HideInInspector] private string lastTrickName;

    private void OnValidate()
    {
        string assetName = name;
        if (string.IsNullOrEmpty(lastAssetName))
        {
            lastAssetName = assetName;
            lastTrickName = "";
        }

        bool assetNameChanged = lastAssetName != assetName;
        bool trickNameChanged = lastTrickName != TrickName;

        string oldTrickName = lastTrickName;

        lastAssetName = assetName;
        lastTrickName = TrickName;

        // Keep TrickName synced to the asset name when the file is renamed.
        if (assetNameChanged && TrickName != assetName)
        {
            oldTrickName = TrickName;
            TrickName = assetName;
            lastTrickName = TrickName;
            trickNameChanged = true;
            EditorUtility.SetDirty(this);
        }

        if (AllowMultiTap)
        {
            int desiredSize = Mathf.Max(0, maxTaps - 1);

            if (multiTapOverrides == null || multiTapOverrides.Length != desiredSize)
            {
                int oldSize = multiTapOverrides?.Length ?? 0;
                Array.Resize(ref multiTapOverrides, desiredSize);

                // Fill newly added entries with defaults.
                for (int i = oldSize; i < desiredSize; i++)
                {
                    int tapCount = i + 2;
                    multiTapOverrides[i].NameOverride = $"{TrickLimits.GetCountPrefix(tapCount)} {TrickName}";
                    multiTapOverrides[i].PointOverride = PointValue * tapCount;
                }

                EditorUtility.SetDirty(this);
            }

            // Update unchanged/default overrides when TrickName changes.
            if (trickNameChanged && !string.IsNullOrWhiteSpace(oldTrickName))
            {
                for (int i = 0; i < multiTapOverrides.Length; i++)
                {
                    int tapCount = i + 2;

                    string oldDefaultName = $"{TrickLimits.GetCountPrefix(tapCount)} {oldTrickName}";
                    string newDefaultName = $"{TrickLimits.GetCountPrefix(tapCount)} {TrickName}";

                    if (string.IsNullOrWhiteSpace(multiTapOverrides[i].NameOverride) ||
                        multiTapOverrides[i].NameOverride == oldDefaultName)
                    {
                        multiTapOverrides[i].NameOverride = newDefaultName;
                        EditorUtility.SetDirty(this);
                    }
                }
            }
        }
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(TricksSO))]
public class TricksSOEditor : Editor
{
    private void OnEnable()
    {
        serializedObject.Update();
        SerializedProperty trickName = serializedObject.FindProperty("TrickName");
        if (trickName != null || string.IsNullOrEmpty(trickName.stringValue))
        {
            trickName.stringValue = target.name;
        }
        serializedObject.ApplyModifiedProperties();
        AssetDatabase.SaveAssets();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty trickName = serializedObject.FindProperty("TrickName");
        SerializedProperty inputString = serializedObject.FindProperty("InputString");
        SerializedProperty button = serializedObject.FindProperty("Button");
        SerializedProperty animationId = serializedObject.FindProperty("AnimationID");
        SerializedProperty pointValue = serializedObject.FindProperty("PointValue");
        SerializedProperty allowMultiTap = serializedObject.FindProperty("AllowMultiTap");
        SerializedProperty multiTapOverrides = serializedObject.FindProperty("multiTapOverrides");
        SerializedProperty maxTaps = serializedObject.FindProperty("maxTaps");
        SerializedProperty holdable = serializedObject.FindProperty("Holdable");
        SerializedProperty secretTrick = serializedObject.FindProperty("SecretTrick");

        bool renameAsset = false;
        string oldTrickName = trickName.stringValue;

        EditorGUI.BeginChangeCheck();
        string newTrickName = EditorGUILayout.DelayedTextField("Trick Name", trickName.stringValue);
        if (EditorGUI.EndChangeCheck())
        {
            trickName.stringValue = newTrickName;
            renameAsset = true;
        }

        EditorGUILayout.PropertyField(inputString, true);
        EditorGUILayout.PropertyField(button);
        EditorGUILayout.PropertyField(animationId);
        EditorGUILayout.PropertyField(pointValue);
        EditorGUILayout.PropertyField(holdable);
        EditorGUILayout.PropertyField(secretTrick);
        EditorGUILayout.Space(10);

        EditorGUILayout.PropertyField(allowMultiTap);

        if (allowMultiTap.boolValue)
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.PropertyField(maxTaps);

            int desiredSize = Mathf.Max(0, maxTaps.intValue - 1);
            int oldSize = multiTapOverrides.arraySize;

            if (oldSize != desiredSize)
            {
                multiTapOverrides.arraySize = desiredSize;

                // Auto-fill any newly added entries.
                if (desiredSize > oldSize)
                {
                    for (int i = oldSize; i < desiredSize; i++)
                    {
                        SerializedProperty entry = multiTapOverrides.GetArrayElementAtIndex(i);
                        SerializedProperty nameOverride = entry.FindPropertyRelative("NameOverride");
                        SerializedProperty pointOverride = entry.FindPropertyRelative("PointOverride");

                        int tapCount = i + 2;
                        nameOverride.stringValue = $"{TrickLimits.GetCountPrefix(tapCount)} {trickName.stringValue}";
                        pointOverride.intValue = pointValue.intValue * TrickLimits.GetPointMult(tapCount);
                    }
                }
            }

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Multi-Tap Overrides", EditorStyles.boldLabel);

            for (int i = 0; i < multiTapOverrides.arraySize; i++)
            {
                SerializedProperty entry = multiTapOverrides.GetArrayElementAtIndex(i);
                SerializedProperty nameOverride = entry.FindPropertyRelative("NameOverride");
                SerializedProperty pointOverride = entry.FindPropertyRelative("PointOverride");

                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.LabelField($"{TrickLimits.GetCountPrefix(i + 2)} Override", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(nameOverride, new GUIContent("Name Override"));
                EditorGUILayout.PropertyField(pointOverride, new GUIContent("Point Override"));
                EditorGUILayout.EndVertical();
            }
        }

        serializedObject.ApplyModifiedProperties();
        if (renameAsset && !string.IsNullOrWhiteSpace(trickName.stringValue) && oldTrickName != trickName.stringValue)
        {
            string path = AssetDatabase.GetAssetPath(target);
            string error = AssetDatabase.RenameAsset(path, trickName.stringValue);

            if (!string.IsNullOrEmpty(error))
                Debug.LogError(error, target);
            else
                AssetDatabase.SaveAssets();
        }
    }
}
#endif