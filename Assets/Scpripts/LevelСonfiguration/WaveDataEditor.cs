using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(WaveData))]
public class WaveDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var waves = serializedObject.FindProperty("Waves");

        for (int i = 0; i < waves.arraySize; i++)
        {
            DrawWave(waves.GetArrayElementAtIndex(i), i + 1);
        }

        if (GUILayout.Button("Add Wave"))
        {
            waves.arraySize++;
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawWave(SerializedProperty wave, int waveIndex)
    {
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            EditorGUILayout.LabelField($"Wave {waveIndex}", EditorStyles.boldLabel);

            var enemies = wave.FindPropertyRelative("Enemies");
            EditorGUILayout.LabelField("Enemies");
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                for (int j = 0; j < enemies.arraySize; j++)
                {
                    var enemy = enemies.GetArrayElementAtIndex(j);
                    EditorGUILayout.PropertyField(enemy.FindPropertyRelative("EnemyPrefab"), new GUIContent("Enemy Prefab"));
                    EditorGUILayout.PropertyField(enemy.FindPropertyRelative("Count"), new GUIContent("Count"));
                    EditorGUILayout.PropertyField(enemy.FindPropertyRelative("Interval"), new GUIContent("Interval"));
                }
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Remove Enemy") && enemies.arraySize > 0) enemies.arraySize--;
                    if (GUILayout.Button("Add Enemy")) enemies.arraySize++;
                }
            }

            EditorGUILayout.PropertyField(wave.FindPropertyRelative("Reward"), new GUIContent("Reward"));
            var useCircleSpawn = wave.FindPropertyRelative("UseCircleSpawn");
            EditorGUILayout.PropertyField(useCircleSpawn, new GUIContent("Use Circle Spawn"));

            if (useCircleSpawn.boolValue)
            {
                EditorGUILayout.PropertyField(wave.FindPropertyRelative("CircleSpawnRadius"), new GUIContent("Circle Spawn Radius"));
            }
            else
            {
                var useRandomFronts = wave.FindPropertyRelative("UseRandomFronts");
                EditorGUILayout.PropertyField(useRandomFronts, new GUIContent("Use Random Fronts"));
                if (!useRandomFronts.boolValue)
                {
                    DrawSpawnPoints(wave);
                }
            }
        }
    }

    private void DrawSpawnPoints(SerializedProperty wave)
    {
        var spawnFrontPrefab = wave.FindPropertyRelative("SpawnFrontPrefab");
        EditorGUILayout.PropertyField(spawnFrontPrefab, new GUIContent("Spawn Front Prefab"));
        var prefab = spawnFrontPrefab.objectReferenceValue as GameObject;

        if (prefab != null)
        {
            var map = prefab.GetComponent<SpawnPointMap>();
            if (map == null)
            {
                EditorGUILayout.HelpBox($"No SpawnPointMap on {prefab.name}!", MessageType.Error);
                return;
            }

            var points = map.GetSpawnPoints();
            if (points == null || points.Count == 0)
            {
                EditorGUILayout.HelpBox($"No spawn points in {prefab.name}!", MessageType.Warning);
                return;
            }

            var spawnPointIndices = wave.FindPropertyRelative("SpawnPointIndices");
            EditorGUILayout.LabelField("Spawn Points");
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                var pointNames = new string[points.Count];
                for (int j = 0; j < points.Count; j++)
                {
                    pointNames[j] = points[j] != null ? points[j].name : $"Point {j + 1}";
                }

                spawnPointIndices.arraySize = EditorGUILayout.IntField("Point Count", spawnPointIndices.arraySize);

                for (int j = 0; j < spawnPointIndices.arraySize; j++)
                {
                    var indexProp = spawnPointIndices.GetArrayElementAtIndex(j);
                    int selectedIndex = indexProp.intValue;
                    selectedIndex = EditorGUILayout.Popup($"Point {j + 1}", selectedIndex, pointNames);
                    if (selectedIndex >= 0 && selectedIndex < points.Count)
                    {
                        indexProp.intValue = selectedIndex;
                    }
                    else
                    {
                        EditorGUILayout.HelpBox($"Invalid index {selectedIndex}, max: {points.Count - 1}", MessageType.Warning);
                    }
                }
            }
        }
    }
}