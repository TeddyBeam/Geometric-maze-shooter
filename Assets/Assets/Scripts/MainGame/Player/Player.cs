using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BaseSystems.DesignPatterns.Observer;
using MainGame.Guns;

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
        private Text healthDisplay;

        [SerializeField]
        private Slider healthBar;

        [SerializeField]
        private PlayerController playerController;

        [SerializeField]
        private GunController gunController;

        [SerializeField]
        private Camera mainCamera;

        protected override void Start()
        {
            base.Start();

            /// Set up the health UI.
            healthBar.maxValue = startingHealth;
            healthBar.value = startingHealth;
            healthDisplay.text = "Health: " + healthBar.value;
        }

        protected virtual void Update()
        {
            /// Move
            Vector3 moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
            playerController.Move(moveInput * moveSpeed);

            /// Rotate
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray.origin, ray.direction, out hit, rayCastMaxDistance, inputLayers))
            {
                Debug.DrawRay(ray.origin, ray.direction * rayCastMaxDistance, Color.red);

                /// Shot
                if (Input.GetMouseButtonDown(0))
                {
                    gunController.Shoot();
                }

                playerController.Rotate(hit.point);
            }
        }

        public override void OnBeingAttacked(int damage, Vector3 hitPoint = default(Vector3), Vector3 hitDirection = default(Vector3))
        {
            healthBar.value -= damage;
            healthDisplay.text = "Health: " + healthBar.value;
            base.OnBeingAttacked(damage, hitPoint, hitDirection);
        }

        protected override void Die()
        {
            SingletonEventDispatcher.Instance.PostEvent(EventsID.OnGameOver);
            base.Die();
        }
    }
}
