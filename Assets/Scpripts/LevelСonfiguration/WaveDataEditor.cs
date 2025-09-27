//  астомный редактор дл€ WaveData: рендерит волны и точки спавна.
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(WaveData))]
public class WaveDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        SerializedProperty waves = serializedObject.FindProperty("Waves");

        for (int i = 0; i < waves.arraySize; i++)
        {
            SerializedProperty wave = waves.GetArrayElementAtIndex(i);
            SerializedProperty enemies = wave.FindPropertyRelative("Enemies");
            SerializedProperty reward = wave.FindPropertyRelative("Reward");
            SerializedProperty useCircleSpawn = wave.FindPropertyRelative("UseCircleSpawn");
            SerializedProperty circleSpawnRadius = wave.FindPropertyRelative("CircleSpawnRadius");
            SerializedProperty spawnFrontPrefab = wave.FindPropertyRelative("SpawnFrontPrefab");
            SerializedProperty spawnPointIndices = wave.FindPropertyRelative("SpawnPointIndices");
            SerializedProperty useRandomFronts = wave.FindPropertyRelative("UseRandomFronts");

            EditorGUILayout.LabelField($"Wave {i + 1}", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            // –ендерим Enemies вручную дл€ избежани€ ошибок с AnimationCurve
            EditorGUILayout.LabelField("Enemies");
            EditorGUI.indentLevel++;
            for (int j = 0; j < enemies.arraySize; j++)
            {
                SerializedProperty enemy = enemies.GetArrayElementAtIndex(j);
                SerializedProperty enemyPrefab = enemy.FindPropertyRelative("EnemyPrefab");
                SerializedProperty count = enemy.FindPropertyRelative("Count");
                SerializedProperty interval = enemy.FindPropertyRelative("Interval");
                SerializedProperty intervalCurve = enemy.FindPropertyRelative("IntervalCurve");
                SerializedProperty useCurve = enemy.FindPropertyRelative("UseCurve");

                EditorGUILayout.PropertyField(enemyPrefab, new GUIContent("Enemy Prefab"));
                EditorGUILayout.PropertyField(count, new GUIContent("Count"));
                EditorGUILayout.PropertyField(useCurve, new GUIContent("Use Curve"));
                if (useCurve.boolValue)
                {
                    EditorGUILayout.PropertyField(intervalCurve, new GUIContent("Interval Curve"));
                }
                else
                {
                    EditorGUILayout.PropertyField(interval, new GUIContent("Interval"));
                }
            }
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+")) enemies.arraySize++;
            if (GUILayout.Button("-") && enemies.arraySize > 0) enemies.arraySize--;
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;

            EditorGUILayout.PropertyField(reward, new GUIContent("Reward"));
            EditorGUILayout.PropertyField(useCircleSpawn, new GUIContent("Use Circle Spawn"));

            if (useCircleSpawn.boolValue)
            {
                EditorGUILayout.PropertyField(circleSpawnRadius, new GUIContent("Circle Spawn Radius"));
            }
            else
            {
                EditorGUILayout.PropertyField(useRandomFronts, new GUIContent("Use Random Fronts"));
                if (!useRandomFronts.boolValue)
                {
                    EditorGUILayout.PropertyField(spawnFrontPrefab, new GUIContent("Spawn Front Prefab"));
                    GameObject prefab = (GameObject)spawnFrontPrefab.objectReferenceValue;

                    if (prefab != null)
                    {
                        SpawnPointMap map = prefab.GetComponent<SpawnPointMap>();
                        if (map == null)
                        {
                            EditorGUILayout.HelpBox($"No SpawnPointMap on {prefab.name}!", MessageType.Error);
                        }
                        else
                        {
                            List<Transform> points = map.GetSpawnPoints();
                            if (points == null || points.Count == 0)
                            {
                                EditorGUILayout.HelpBox($"No spawn points in {prefab.name}!", MessageType.Warning);
                            }
                            else
                            {
                                EditorGUILayout.LabelField("Spawn Points");
                                EditorGUI.indentLevel++;
                                spawnPointIndices.arraySize = EditorGUILayout.IntField("Point Count", spawnPointIndices.arraySize);
                                for (int j = 0; j < spawnPointIndices.arraySize; j++)
                                {
                                    int index = spawnPointIndices.GetArrayElementAtIndex(j).intValue;
                                    index = EditorGUILayout.IntField($"Point {j + 1}", index);
                                    if (index >= 0 && index < points.Count)
                                        spawnPointIndices.GetArrayElementAtIndex(j).intValue = index;
                                    else
                                        EditorGUILayout.HelpBox($"Invalid index {index}, max: {points.Count - 1}", MessageType.Warning);
                                }
                                EditorGUI.indentLevel--;
                            }
                        }
                    }
                }
            }
            EditorGUI.indentLevel--;
        }

        if (GUILayout.Button("Add Wave"))
        {
            waves.arraySize++;
        }

        serializedObject.ApplyModifiedProperties();
    }
}