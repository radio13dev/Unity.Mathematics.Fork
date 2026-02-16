using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEditor;
using UnityEngine;


namespace Unity.Mathematics.Fixed.Editor
{
    [CustomPropertyDrawer(typeof(fp))]
    class fpDrawer : PropertyDrawer
    {
#if !UNITY_2023_2_OR_NEWER
        public override bool CanCacheInspectorGUI(SerializedProperty property)
        {
            return false;
        }
#endif

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var valueProp = property.FindPropertyRelative("value");
            long raw = valueProp.longValue;
            fp fpValue = fp.ParseRaw(raw);
            float floatValue = (float)fpValue;

            Rect fieldRect = EditorGUI.PrefixLabel(position, label);

            EditorGUI.BeginChangeCheck();
            float newFloat = EditorGUI.FloatField(fieldRect, floatValue);
            if (EditorGUI.EndChangeCheck())
            {
                fp newFP = fp.ParseUnsafe(newFloat);
                valueProp.longValue = newFP.value;
                property.serializedObject.ApplyModifiedProperties();
            }

            EditorGUI.EndProperty();
        }
    }
}
