using System;
using UnityEngine;

namespace Extension
{
    namespace Attributes
    {
        /// <summary>
        /// Specifies a field to use as label for an item in the inspector. This is especially useful for arrays of compound types.
        /// </summary>
        /// <example>
        /// In this example, the "type" field of ArrayItem will be used as the item
        /// label for the array in the inspector.
        /// [Serializable]
        /// public enum EnumNames
        /// {
        ///	  Label1,
        ///	  Label2
        /// }
        ///
        /// [Serializable]
        /// public class ArrayItem
        /// {
        ///	  public EnumNames type;
        ///   public int value; 
        /// }
        ///
        /// public class LabelFieldTest : MonoBehaviour
        /// {
        ///   [LabelField("type")]
        ///   public ArrayItem[] items;
        /// }
        /// </example>
        [AttributeUsage(AttributeTargets.Field)]
        public class LabelFieldAttribute : PropertyAttribute
        {
            private readonly string labelField;

            public LabelFieldAttribute(string labelField)
            {
                this.labelField = labelField;
            }

            public string LabelField
            {
                get { return labelField; }
            }
        }
    }
}