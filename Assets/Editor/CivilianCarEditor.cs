using UnityEditor;
using UnityEngine;

/// <summary>
/// Civilian car custom editor for showing all the possible lane options in the civilian car game object's inspector.
/// </summary>
[CustomEditor(typeof(CivilianCar))]
[CanEditMultipleObjects]
public class CivilianCarEditor : Editor
{
    // Properties to sync with the game object's corresponding variables
    SerializedProperty laneOptionsProp;
    SerializedProperty laneIndexProp;


    // Use this for enabling
    private void OnEnable()
    {
        laneOptionsProp = serializedObject.FindProperty("laneOptions");
        laneIndexProp = serializedObject.FindProperty("laneIndex");
    }

    // Use this for what to show when the contents are laid out in editor
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        base.OnInspectorGUI();
        string[] laneOptions = getLaneOptions();
        laneIndexProp.intValue = EditorGUILayout.Popup("Lane", laneIndexProp.intValue, laneOptions);
        serializedObject.ApplyModifiedProperties();
    }


    // Returns all the available lane options
    private string[] getLaneOptions()
    {
        string[] laneOptions = null;

        if (laneOptionsProp.isArray)
        {
            int arraySize = laneOptionsProp.arraySize;
            laneOptions = new string[arraySize];

            for (int i = 0; i < laneOptions.Length; i++)
            {
                string laneOption = laneOptionsProp.GetArrayElementAtIndex(i).stringValue;
                laneOptions[i] = laneOption;
            }
        }

        return laneOptions;
    }
}
