using UnityEngine;
using UnityEditor;
//В инспекторе добвил кнопку для удоной генерации сетки в сццене
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