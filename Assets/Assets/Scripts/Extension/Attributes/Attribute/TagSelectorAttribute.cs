using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Extension
{
    namespace Attributes
    {
        /// <summary>
        /// Used with a string to create a tag dropbox list.
        /// </summary>
        public class TagSelectorAttribute : PropertyAttribute
        {
            public bool UseDefaultTagFieldDrawer = false;
        }
    }
}
