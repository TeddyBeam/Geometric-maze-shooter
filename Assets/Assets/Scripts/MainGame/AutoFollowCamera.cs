using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MainGame
{
    public class AutoFollowCamera : MonoBehaviour
    {
        [SerializeField]
        private Transform target;

        [SerializeField]
        private Camera mainCamera;

        [SerializeField, Range(0.01f, 2f)]
        private float minDistance = 0.05f;

        [SerializeField]
        private float smoothingValue = 5f;

        private Vector3 offset;

        protected virtual void Start()
        {
            offset = mainCamera.transform.position - target.position;
        }

        protected virtual void LateUpdate()
        {
            if (offset.magnitude > minDistance && target != null)
            {
                Vector3 targetCamPos = target.position + offset;
                mainCamera.transform.position = Vector3.Lerp(transform.position, targetCamPos, smoothingValue * Time.deltaTime);
            }
        }
    }
}
