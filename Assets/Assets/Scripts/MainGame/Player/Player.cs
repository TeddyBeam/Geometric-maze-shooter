using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BaseSystems.Observer;

namespace MainGame
{
    public class Player : LivingObject
    {
        [SerializeField, Range(0.1f, 10f)]
        private float moveSpeed = 5f;

        [SerializeField, Range(1, 100f)]
        private float rayCastMaxDistance = 100f;

        [SerializeField]
        private LayerMask inputLayers = 1; // 1 = everything.

        [SerializeField]
        private PlayerController playerController;

        [SerializeField]
        private PlayerWeapon gunController;

        [SerializeField]
        private Camera mainCamera;

        protected override void Start()
        {
            base.Start();
        }

        protected virtual void Update()
        {
            /// Move
            Vector3 moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
            Vector3 moveVelocity = moveInput.normalized * moveSpeed;
            playerController.Move(moveVelocity);

            /// Rotate
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray.origin, ray.direction, out hit, rayCastMaxDistance, inputLayers))
            {
                Debug.DrawRay(ray.origin, ray.direction * rayCastMaxDistance, Color.red);
                playerController.Rotate(hit.point);
            }

            /// Shot
            if (Input.GetMouseButtonDown(0))
            {
                gunController.Shoot();
            }
        }

        protected override void Die()
        {
            this.PostEvent(ObserverEventID.OnGameOver);
            base.Die();
        }
    }
}
