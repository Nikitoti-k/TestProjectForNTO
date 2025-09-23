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

            // Рендерим поля Enemies и Reward
            EditorGUILayout.PropertyField(enemies, new GUIContent("Enemies"));
            EditorGUILayout.PropertyField(reward, new GUIContent("Reward"));

            // Рендерим поля спавна
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
                        if (map != null)
                        {
                            List<Transform> points = map.GetSpawnPoints();
                            if (points == null || points.Count == 0)
                            {
                                EditorGUILayout.HelpBox($"No spawn points in {prefab.name}!", MessageType.Warning);
                            }
                            else
                            {
                                List<string> pointNames = new List<string>();
                                for (int j = 0; j < points.Count; j++)
                                {
                                    pointNames.Add(points[j].name);
                                }

                                // Множественный выбор точек
                                int[] selectedIndices = spawnPointIndices.arraySize > 0 ? new int[spawnPointIndices.arraySize] : new int[0];
                                for (int j = 0; j < selectedIndices.Length; j++)
                                {
                                    selectedIndices[j] = spawnPointIndices.GetArrayElementAtIndex(j).intValue;
                                }

                                EditorGUILayout.LabelField("Spawn Points");
                                EditorGUI.indentLevel++;
                                for (int j = 0; j < pointNames.Count; j++)
                                {
                                    bool isSelected = System.Array.IndexOf(selectedIndices, j) >= 0;
                                    bool newSelected = EditorGUILayout.Toggle(pointNames[j], isSelected);
                                    if (newSelected != isSelected)
                                    {
                                        if (newSelected)
                                        {
                                            spawnPointIndices.arraySize++;
                                            spawnPointIndices.GetArrayElementAtIndex(spawnPointIndices.arraySize - 1).intValue = j;
                                        }
                                        else
                                        {
                                            for (int k = 0; k < spawnPointIndices.arraySize; k++)
                                            {
                                                if (spawnPointIndices.GetArrayElementAtIndex(k).intValue == j)
                                                {
                                                    spawnPointIndices.DeleteArrayElementAtIndex(k);
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                                EditorGUI.indentLevel--;
                            }
                        }
                        else
                        {
                            EditorGUILayout.HelpBox($"No SpawnPointMap component on {prefab.name}!", MessageType.Error);
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