using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extension.Attributes;

namespace BaseSystems.Utilities
{
    public class DestroyAfterTime : MonoBehaviour
    {
        [SerializeField, MinMaxSlider(1f, 30f)]
        private Vector2 delayTime = new Vector2(7.5f, 10f);

        protected virtual void Start ()
        {
            StartCoroutine(DestroyCountDown(Random.Range(delayTime.x, delayTime.y)));
        }

        private IEnumerator DestroyCountDown(float delayTime)
        {
            yield return new WaitForSeconds(delayTime);
            Destroy(gameObject);
            yield break;
        }
    }
}
