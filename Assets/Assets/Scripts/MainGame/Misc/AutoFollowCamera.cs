using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MainGame.Misc
{
    public class AutoFollowCamera : MonoBehaviour
    {
        [SerializeField]
        private Transform target;

        [SerializeField]
        private Camera mainCamera;

        [SerializeField]
        private float smoothingValue = 5f;

        private Vector3 offset;

        protected virtual void Start()
        {
            offset = mainCamera.transform.position - target.position;
        }

        protected virtual void LateUpdate()
        {
            if (target != null)
            {
                Vector3 targetCamPos = target.position + offset;
                mainCamera.transform.position = Vector3.Lerp(transform.position, targetCamPos, smoothingValue * Time.deltaTime);
            }
        }
    }
}
