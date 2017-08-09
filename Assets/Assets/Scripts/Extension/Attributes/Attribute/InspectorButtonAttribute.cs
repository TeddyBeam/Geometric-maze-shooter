using System;
using UnityEngine;

namespace Extension
{
    namespace Attributes
    {
        /// <summary>
        /// This attribute can only be applied to fields because its associated PropertyDrawer only operates on fields (either
        /// public or tagged with the [SerializeField] attribute) in the target MonoBehaviour.
        /// </summary>
        [AttributeUsage(AttributeTargets.Method)]
        public class InspectorButtonAttribute : PropertyAttribute
        {
            /*
            public static float kDefaultButtonWidth = 80;

            public readonly string MethodName;

            private float _buttonWidth = kDefaultButtonWidth;
            public float ButtonWidth
            {
                get { return _buttonWidth; }
                set { _buttonWidth = value; }
            }

            public InspectorButtonAttribute(string MethodName)
            {
                this.MethodName = MethodName;
            }
            */
        }
    }
}