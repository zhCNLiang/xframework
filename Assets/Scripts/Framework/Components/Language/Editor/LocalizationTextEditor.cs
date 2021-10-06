using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System;

[CustomEditor(typeof(LocalizationText))]
public class LocalizationTextEditor : Editor
{
    private SerializedProperty m_KeyProperty;
    private SerializedProperty m_LanguageProperty;
    private Text m_Text;
    private int m_LastIndex;

    void OnEnable()
    {
        m_KeyProperty = serializedObject.FindProperty("m_Key");
        m_LanguageProperty = serializedObject.FindProperty("m_Language");
        m_Text = (target as LocalizationText)?.GetComponent<Text>();
        m_LastIndex = m_LanguageProperty.intValue;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var bRefresh = false;

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("Key:");
        var key = EditorGUILayout.TextField(m_KeyProperty.stringValue);
        if (key != m_KeyProperty.stringValue)
        {
            m_KeyProperty.stringValue = key;
            bRefresh = true;
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("Language:");

        var items = new List<GUIContent>();
        var languages = new List<LocalizationLanguage>();
        foreach(LocalizationLanguage e in Enum.GetValues(typeof(LocalizationLanguage)))
        {
            if (LocalizationService.Instance.IsSupport(e))
            {
                languages.Add(e);
                items.Add(new GUIContent(e.ToString()));
            }
        }

        var index = EditorGUILayout.Popup(m_LastIndex, items.ToArray());
        index = index >= languages.Count ? 0 : index;
        if (index != m_LastIndex)
        {
            m_LastIndex = index;
            m_LanguageProperty.intValue = index;
            LocalizationService.Instance.Language = languages[index];
            bRefresh = true;
        }

        EditorGUILayout.EndHorizontal();

        if (m_Text != null && bRefresh)
        {
            var text = LocalizationService.Instance.GetValue(key);
            m_Text.text = text;
        }

        if (!LocalizationService.Instance.HasKey(key))
        {
            var style = GUIStyle.none;
            style.normal.textColor = Color.red;
            EditorGUILayout.LabelField("Key Not Found.", style);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
