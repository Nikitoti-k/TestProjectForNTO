using UnityEngine;
using UnityEditor;
// такое обычно нейросетям отдаю на чисто механическую работу, потому что оформить SO красиво - хочется, а писать вручную - не хочется)
[CustomEditor(typeof(GameSceneConfiguration))]
public class GameSceneConfigurationEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Grid Settings", EditorStyles.boldLabel);
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("gridRadius"), new GUIContent("Grid Radius"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("cellSize"), new GUIContent("Cell Size"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("gridLineMaterial"), new GUIContent("Grid Line Material"));
        }

        if (GUILayout.Button("Generate Hex Grid"))
        {
            GenerateHexGrid();
        }

        EditorGUILayout.LabelField("Building Placement Settings", EditorStyles.boldLabel);
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("buildingScaleFactor"), new GUIContent("Building Scale Factor"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("headquartersPrefab"), new GUIContent("Headquarters Prefab"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("previewValidMaterial"), new GUIContent("Preview Valid Material"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("previewInvalidMaterial"), new GUIContent("Preview Invalid Material"));
        }

        EditorGUILayout.LabelField("Economy Settings", EditorStyles.boldLabel);
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("startingCurrency"), new GUIContent("Starting Currency"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("sellPriceMultiplier"), new GUIContent("Sell Price Multiplier"));
        }

        EditorGUILayout.LabelField("Interaction Settings", EditorStyles.boldLabel);
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("buildingLayer"), new GUIContent("Building Layer"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("enemyLayer"), new GUIContent("Enemy Layer"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("spawnRadius"), new GUIContent("Spawn Radius"));
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void GenerateHexGrid()
    {
        var settings = (GameSceneConfiguration)target;
        var grid = FindFirstObjectByType<HexGrid>();
        if (grid != null)
        {
            settings.GenerateGrid(grid);
            EditorUtility.SetDirty(grid);
            EditorUtility.SetDirty(target);
        }
        else
        {
            Debug.LogError("GameSceneConfigurationEditor: HexGrid not found in scene!");
        }
    }
}