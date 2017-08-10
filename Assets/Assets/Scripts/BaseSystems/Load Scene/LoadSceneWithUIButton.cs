using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Extension.Attributes;
using Extension.ExtraTypes;

namespace BaseSystems.SceneHelpers
{
    /// <summary>
    /// Add load scene events into UI buttons.
    /// </summary>
    public sealed class LoadSceneWithUIButton : MonoBehaviour
    {
        [Serializable]
        public class LoadSceneWithUIButtonConfig
        {
            public Button button;
            public string targetSceneName;
        }

        [Serializable]
        public class LoadSceneWithUIButtonConfigList : ReorderableArray<LoadSceneWithUIButtonConfig> { }

        [SerializeField, Reorderable("targetSceneName")]
        private LoadSceneWithUIButtonConfigList buttonList;

        private void Awake ()
        {
            foreach (var config in buttonList)
            {
                config.button.onClick.AddListener(() => SceneManager.LoadScene(config.targetSceneName));
            }
        }
    }
}
