using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BuildingData))]
public class BuildingDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        BuildingData data = (BuildingData)target;
        serializedObject.Update(); // Синхронизируем данные перед редактированием

        // Рисуем поле Levels
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Levels"), true);

        // Рисуем поле Modules
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Modules"), true);

        // Проверка синхронизации уровней для каждого модуля
        if (data.Modules != null)
        {
            for (int i = 0; i < data.Modules.Count; i++)
            {
                var module = data.Modules[i];
                if (module == null)
                {
                    EditorGUILayout.HelpBox($"Module at index {i} is null! Please assign a valid module.", MessageType.Error);
                    continue;
                }

                var so = new SerializedObject(module);
                var levelData = so.FindProperty("LevelData");
                if (levelData != null && levelData.arraySize != data.Levels.Count)
                {
                    EditorGUILayout.HelpBox($"Module {module.name} has {levelData.arraySize} levels, but BuildingData has {data.Levels.Count} levels!", MessageType.Warning);
                }
            }
        }

        // Применяем изменения и сохраняем
        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
            foreach (var module in data.Modules)
            {
                if (module != null)
                {
                    EditorUtility.SetDirty(module); // Сохраняем изменения в модулях
                }
            }
        }
        serializedObject.ApplyModifiedProperties(); // Применяем изменения
    }
}