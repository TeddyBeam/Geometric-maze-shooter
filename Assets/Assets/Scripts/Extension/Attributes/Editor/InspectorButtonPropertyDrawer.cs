#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
using UnityEngine;
using System;
using System.Linq;

namespace Extension
{
    namespace Attributes
    {
        // [CustomPropertyDrawer(typeof(InspectorButtonAttribute))]
        [CustomEditor(typeof(MonoBehaviour), true)]
        public class InspectorButtonPropertyDrawer : Editor // its not a PropertyDrawer anymore...
        {
            public override void OnInspectorGUI ()
            {
                base.OnInspectorGUI();

                var mono = target as MonoBehaviour;

                var methods = mono.GetType()
                    .GetMembers(BindingFlags.Instance | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                                BindingFlags.NonPublic)
                    .Where(o => Attribute.IsDefined(o, typeof(InspectorButtonAttribute)));

                foreach (var memberInfo in methods)
                {
                    if (GUILayout.Button(memberInfo.Name))
                    {
                        var method = memberInfo as MethodInfo;
                        method.Invoke(mono, null);
                    }
                }
            }
        }
    }
}
#endif