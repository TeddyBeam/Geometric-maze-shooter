#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
using UnityEngine;
using System;
using System.Linq;

namespace Extension.Attributes
{
    [CustomPropertyDrawer(typeof(MinMaxSliderAttribute))]
    class MinMaxSliderDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.Vector2)
            {
                Vector2 range = property.vector2Value;
                float min = range.x;
                float max = range.y;
                MinMaxSliderAttribute attr = attribute as MinMaxSliderAttribute;
                EditorGUI.BeginChangeCheck();          
                    
                EditorGUI.MinMaxSlider(position, ref min, ref max, attr.min, attr.max);
                EditorGUILayout.LabelField(property.displayName);
                EditorGUILayout.LabelField("Min Value:", min.ToString("n2"));
                EditorGUILayout.LabelField("Max Value:", max.ToString("n2"));

                if (EditorGUI.EndChangeCheck())
                {
                    range.x = min;
                    range.y = max;
                    property.vector2Value = range;
                }
            }
            else
            {
                EditorGUI.LabelField(position, label, "Use only with Vector2");
            }
        }
    }
}
#endif
