using System;
using UnityEngine;
using BaseSystems.Observer;
using Extension.ExtraTypes;
using UnityEngine.SceneManagement;

namespace BaseSystems.SceneHelpers
{
    /// <summary>
    /// Load scene with registerd observer events.
    /// </summary>
    public sealed class LoadSceneWithObserver : MonoBehaviour
    {
        [SerializeField]
        private ObserverEventStringDictionary eventsDict;

        private void Awake()
        {
            foreach (ObserverEventID key in eventsDict.Keys)
            {
                this.RegisterListener(key,  _=> SceneManager.LoadScene(eventsDict[key]));
            }
        }
    }
}
