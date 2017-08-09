using System;
using UnityEngine;

namespace Extension
{
    namespace Attributes
    {
        /// <summary>
        /// Used to mark a field to add a comment above the field in the inspector.
        /// </summary>
        [AttributeUsage(AttributeTargets.Field)]
        public class CommentAttribute : PropertyAttribute
        {
            public readonly GUIContent content;
            public CommentAttribute(string comment, string tooltip = "")
            {
                content = string.IsNullOrEmpty(tooltip) ? new GUIContent(comment) : new GUIContent(comment + " [?]", tooltip);
            }
        }
    }
}