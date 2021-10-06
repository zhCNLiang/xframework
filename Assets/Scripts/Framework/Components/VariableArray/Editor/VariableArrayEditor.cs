using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(VariableArray))]
public class VariableArrayEditor : Editor
{
    private SerializedProperty m_VarObjectsProperty;
    void OnEnable()
    {
        m_VarObjectsProperty = serializedObject.FindProperty("m_VarObjects");
        CleanNullVariable();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        for(int i=0; i<m_VarObjectsProperty.arraySize; i++)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(m_VarObjectsProperty.GetArrayElementAtIndex(i));
            if (i > 0 && GUILayout.Button("↑", GUILayout.Width(20)))
            {
                m_VarObjectsProperty.MoveArrayElement(i, i - 1);
                break;
            }
            if (i < m_VarObjectsProperty.arraySize - 1 && GUILayout.Button("↓", GUILayout.Width(20)))
            {
                m_VarObjectsProperty.MoveArrayElement(i, i + 1);
                break;
            }
            if (GUILayout.Button("-", GUILayout.Width(20)))
            {
                m_VarObjectsProperty.DeleteArrayElementAtIndex(i);
                break;
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5);
        }
        GUILayout.Space(5);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Clean Variable"))
        {
            CleanNullVariable();
        }

        if (GUILayout.Button("Add Variable"))
        {
            m_VarObjectsProperty.InsertArrayElementAtIndex(m_VarObjectsProperty.arraySize);
        }
        EditorGUILayout.EndHorizontal();
        serializedObject.ApplyModifiedProperties();
    }

    private void CleanNullVariable()
    {
        for (int i=0; i<m_VarObjectsProperty.arraySize; i++)
        {
            var serializedProperty = m_VarObjectsProperty.GetArrayElementAtIndex(i);
            serializedProperty = serializedProperty.FindPropertyRelative("Obj");
            if (null == serializedProperty.objectReferenceValue)
            {
                m_VarObjectsProperty.DeleteArrayElementAtIndex(i);
                i--;
            }
        }
        serializedObject.ApplyModifiedProperties();
    }
}
