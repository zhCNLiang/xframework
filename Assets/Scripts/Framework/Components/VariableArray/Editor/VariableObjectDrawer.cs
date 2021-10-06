using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[CustomPropertyDrawer(typeof(VariableObject))]
public class VariableObjectDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        EditorGUI.PropertyField(new Rect(position.x, position.y, 110, position.height), property.FindPropertyRelative("Name"), GUIContent.none);

        List<Type> types = new List<Type>();
        List<string> typeNames = new List<string>();

        Type type;
        string typeName;

        var p = property.FindPropertyRelative("Obj");
        if (p != null && p.objectReferenceValue != null)
        {
            var obj = p.objectReferenceValue;

            var go = obj as Component ? ((Component)obj).gameObject : (GameObject)obj;

            type = go.GetType();
            typeName = type.FullName;
            types.Add(type);
            typeNames.Add(typeName);

            foreach(var e in go.GetComponents<Component>())
            {
                type = e.GetType();
                typeName = type.FullName;
                types.Add(type);
                typeNames.Add(typeName);
            }
            typeName = obj.GetType().FullName;
        }
        else
        {
            typeNames.Add("None");
            typeName = "None";
        }

        var index = Math.Max(typeNames.IndexOf(typeName), 0);
        index = Math.Min(index, typeNames.Count - 1);
        var newindex = EditorGUI.Popup(new Rect(position.x + 120, position.y, 105, 30), index, typeNames.ToArray());
        if (newindex != index)
        {
            index = newindex;
            var t = typeNames.Count > 0 && index < typeNames.Count ? types[index] : typeof(UnityEngine.Object);

            var obj = p.objectReferenceValue;
            var go = obj as Component ? ((Component)obj).gameObject : (GameObject)obj;
            p.objectReferenceValue = t == typeof(GameObject) ? (UnityEngine.Object)go : (UnityEngine.Object)go.GetComponent(t);
        }

        p.objectReferenceValue = EditorGUI.ObjectField(new Rect(position.x + 235, position.y, 110, position.height), p.objectReferenceValue, typeof(UnityEngine.Object), true);

        EditorGUI.EndProperty();
    }
}
