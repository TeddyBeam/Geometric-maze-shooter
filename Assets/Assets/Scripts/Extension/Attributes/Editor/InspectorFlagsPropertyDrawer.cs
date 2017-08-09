#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Extension
{
    namespace Attributes
    {
        /// <summary>
        /// A property drawer for fields marked with the InspectorFlags Attribute.
        /// </summary>
        [CustomPropertyDrawer(typeof(InspectorFlagsAttribute))]
        public class InspectorFlagsPropertyDrawer : PropertyDrawer
        {
            public override void OnGUI(Rect position,
                SerializedProperty prop,
                GUIContent label)
            {
                EditorGUI.showMixedValue = prop.hasMultipleDifferentValues;
                EditorGUI.BeginChangeCheck();

                var newValue = EditorGUI.MaskField(position, label, prop.intValue, prop.enumNames);

                if (EditorGUI.EndChangeCheck())
                {
                    prop.intValue = newValue;
                }
            }
        }
    }
}
#endif