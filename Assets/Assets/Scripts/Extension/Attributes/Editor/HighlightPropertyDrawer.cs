#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Extension
{
    namespace Attributes
    {
        /// <summary>
        /// A property drawer for fields marked with the Highlight Attribute.
        /// </summary>
        [CustomPropertyDrawer(typeof(HighlightAttribute))]
        public class HighlightPropertyDrawer : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                var oldColor = GUI.color;
                GUI.color = Color.cyan; // RGBA(52, 152, 219, 255.0) PETER RIVER
                EditorGUI.PropertyField(position, property, label);
                GUI.color = oldColor;
            }
        }
    }
}
#endif