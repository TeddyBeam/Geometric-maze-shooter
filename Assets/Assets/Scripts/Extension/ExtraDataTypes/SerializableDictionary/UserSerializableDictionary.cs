using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BaseSystems.Observer;
using BaseSystems.Data.Storage;

namespace Extension.ExtraTypes
{
    /// Declare custom Serializable Dictionary here. 
    /// Add it into UserSerializableDictionaryPropertyDrawers.
    
    [Serializable]
    public class ObserverEventStringDictionary : SerializableDictionary<ObserverEventID, string> { }

    [Serializable]
    public class StringButtonDictionary : SerializableDictionary<string, Button> { }

    [Serializable]
    public class OberverEventButtonDictionary : SerializableDictionary<ObserverEventID, Button> { }
}
