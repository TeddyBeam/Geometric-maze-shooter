using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extension.Timing;
using Extension.Attributes;
using Extension.ExtraTypes;

namespace BaseSystems.Utilities
{
    /// <summary>
    /// Hide game object after an amount of time.
    /// </summary>
    public sealed class HideAfterTime : MonoBehaviour
    {
        #region Init
        [SerializeField]
        private CoroutineMode coroutineMode = CoroutineMode.UnityNormalCoroutine;

        [SerializeField, MinMaxSlider(0.1f, 300f)]
        private Vector2 randomHideTime = new Vector2(50f, 150f);
        #endregion

        #region Monobehaviours
        void OnEnable()
        {
            // set hide time.
            float hideTime = Random.Range(randomHideTime.x, randomHideTime.y);

            // set coroutine mode.
            if (coroutineMode == CoroutineMode.TimingCoroutine)
            {
                TimingCoroutine.RunCoroutine(TimingCoroutineUse(hideTime).CancelWith(gameObject));
            }
            else // if (coroutineStyle == CoroutineMode.UnityNormalCoroutine)
            {
                StartCoroutine(NormalCoroutineUse(hideTime));
            }
        }

        #endregion

        #region Hide Coroutines
        private IEnumerator NormalCoroutineUse(float hideTime)
        {
            yield return new WaitForSeconds(hideTime);
            gameObject.SetActive(false);
            yield break;
        }

        private IEnumerator<float> TimingCoroutineUse(float hideTime)
        {
            yield return TimingCoroutine.WaitForSeconds(hideTime);
            gameObject.SetActive(false);
            yield break;
        }
        #endregion
    }
}