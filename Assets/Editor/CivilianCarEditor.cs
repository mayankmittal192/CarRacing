using UnityEditor;

/// <summary>
/// Civilian car custom editor for showing all the possible lane options in the civilian car game object's inspector.
/// </summary>
[CustomEditor(typeof(CivilianCar))]
[CanEditMultipleObjects]
public class CivilianCarEditor : Editor
{
    // Properties to sync with the game object's corresponding variables
    SerializedProperty laneOptionsProp;
    SerializedProperty laneOptionIdxProp;

    
    private void OnEnable()
    {
        laneOptionsProp = serializedObject.FindProperty("laneOptions");
        laneOptionIdxProp = serializedObject.FindProperty("laneOptionIdx");
    }


    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        base.OnInspectorGUI();
        string[] laneOptions = getLaneOptions();
        laneOptionIdxProp.intValue = EditorGUILayout.Popup("Lane", laneOptionIdxProp.intValue, laneOptions);
        serializedObject.ApplyModifiedProperties();
    }


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
