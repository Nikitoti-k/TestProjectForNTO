using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(HexGrid))]
public class HexGridEditor : Editor
{
    public override void OnInspectorGUI()
    {
        
        HexGrid hexGrid = (HexGrid)target;

       
        DrawDefaultInspector();

        
        if (GUILayout.Button("Generate Hex Grid"))
        {
            hexGrid.RegenerateGrid();
            EditorUtility.SetDirty(hexGrid); 
        }
    }
}