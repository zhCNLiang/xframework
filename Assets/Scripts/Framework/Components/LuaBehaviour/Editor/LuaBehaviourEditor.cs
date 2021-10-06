using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(LuaBehaviour), true)]
public class LuaBehaviourEditor : Editor
{
    private SerializedProperty m_LuaEntryProperty;
    private void OnEnable()
    {
        m_LuaEntryProperty = serializedObject.FindProperty("luaEntry");
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        {
            serializedObject.Update();
            var path = AppConst.LuaDir + m_LuaEntryProperty.stringValue + AppConst.LuaExt;
            var luaAsset = AssetDatabase.LoadAssetAtPath<LuaAsset>(path);
            var obj = EditorGUILayout.ObjectField("LuaEntry", luaAsset, typeof(LuaAsset), true);
            if (EditorGUI.EndChangeCheck())
            {
                if (obj != null)
                {
                    var assetPath = AssetDatabase.GetAssetPath(obj);
                    assetPath = assetPath.Replace(AppConst.LuaDir, string.Empty);
                    assetPath = assetPath.Replace(AppConst.LuaExt, string.Empty);
                    m_LuaEntryProperty.stringValue = assetPath;
                }
                else
                {
                    m_LuaEntryProperty.stringValue = string.Empty;
                }
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
