using System;
using UnityEngine;

namespace Extension
{
    namespace Attributes
    {
        /// <summary>
        /// Mark fields in a MonoBehaviour with this attribute to give a specific warning 
        /// when the field is not set.
        /// </summary>
        /// 
        /// <example>
        ///	[WarningIfNull("Assign the prefab")]
        ///	public GameObject playerPrefab;
        /// </example>
        [AttributeUsage(AttributeTargets.Field)]
        public class WarningIfNullAttribute : PropertyAttribute
        {
            public string WarningMessage
            {
                get;
                private set;
            }

            /// <summary>
            /// Initializes a new instance of this class.
            /// </summary>
            /// <param name="warningMessage">The warning message to display when the marked field is null.</param>
            public WarningIfNullAttribute()
            {
                WarningMessage = "Null alert";
            }
        }
    }
}
