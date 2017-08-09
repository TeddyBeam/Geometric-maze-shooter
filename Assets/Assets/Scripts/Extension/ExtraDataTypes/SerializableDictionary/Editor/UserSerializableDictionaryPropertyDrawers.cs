#if UNITY_EDITOR
using UnityEditor;

namespace Extension.ExtraTypes
{
    /// <summary>
    /// Declare custom inspector of Serializable Dictionary here.
    /// </summary>
    [CustomPropertyDrawer(typeof(ObserverEventStringDictionary))]
    [CustomPropertyDrawer(typeof(StringButtonDictionary))]
    [CustomPropertyDrawer(typeof(OberverEventButtonDictionary))]
    public class AnySerializableDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
}
#endif
