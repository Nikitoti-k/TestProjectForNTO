// Custom editor для HexGrid - добавляет кнопку regenerate в inspector.
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(HexGrid))]
public class HexGridEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Generate Hex Grid"))
        {
            ((HexGrid)target).RegenerateGrid();
            EditorUtility.SetDirty(target); // Mark dirty для save.
        }
    }
}