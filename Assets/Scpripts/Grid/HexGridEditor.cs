// Custom editor ��� HexGrid - ��������� ������ regenerate � inspector.
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
            EditorUtility.SetDirty(target); // Mark dirty ��� save.
        }
    }
}