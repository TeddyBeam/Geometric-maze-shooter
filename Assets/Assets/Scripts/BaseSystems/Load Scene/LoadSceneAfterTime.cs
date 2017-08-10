using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BaseSystems.SceneHelpers
{
    /// <summary>
    /// Load scene after an amount of time.
    /// </summary>
    public class LoadSceneAfterTime : MonoBehaviour
    {
        [SerializeField]
        private string sceneName = "OpenScene";

        [SerializeField, Range(0.1f, 10f)]
        protected float delayTime = 3.0f;

        protected virtual void Awake()
        {
            StartCoroutine(LoadSceneAfter(sceneName, delayTime));
        }

        protected IEnumerator LoadSceneAfter(string scene, float time)
        {
            yield return new WaitForSeconds(time);
            SceneManager.LoadScene(scene);
            yield break;
        }
    }
}
