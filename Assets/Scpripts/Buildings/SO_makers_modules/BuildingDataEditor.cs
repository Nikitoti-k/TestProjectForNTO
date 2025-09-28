// ��������� �������� ��� BuildingData, ���������� ���� � ����������, ���������������� � ��������
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BuildingData))]
public class BuildingDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        BuildingData data = (BuildingData)target;
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("name"), new GUIContent("Building Name"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxHPDisplayName"), new GUIContent("Max HP Display Name"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("costDisplayName"), new GUIContent("Cost Display Name"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Levels"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Modules"), true);

        // ��������� ������������� ������� ����� BuildingData � ��������
        if (data.Modules != null)
        {
            for (int i = 0; i < data.Modules.Count; i++)
            {
                var module = data.Modules[i];
                if (module == null)
                {
                    EditorGUILayout.HelpBox($"������ �� ������� {i} �� �����!", MessageType.Error);
                    continue;
                }

                var so = new SerializedObject(module);
                var levelData = so.FindProperty("LevelData");
                if (levelData != null && levelData.arraySize != data.Levels.Count)
                {
                    EditorGUILayout.HelpBox($"������ {module.name} ����� {levelData.arraySize} �������, � BuildingData � {data.Levels.Count}!", MessageType.Warning);
                }
            }
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
            foreach (var module in data.Modules)
            {
                if (module != null)
                {
                    EditorUtility.SetDirty(module);
                }
            }
        }
        serializedObject.ApplyModifiedProperties();
    }
}