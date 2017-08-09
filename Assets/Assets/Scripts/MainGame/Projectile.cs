using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BaseSystems.PhysicLogic;

namespace MainGame
{
    public class Projectile : PhysicAttacker<int>
    {
        [SerializeField]
        private LayerMask collisionMask = 1; // 1 = everything

        [SerializeField, Range(0.1f, 1f)]
        private float skinWidth = 0.1f;

        public float Speed { get; set; }

        protected virtual void Start()
        {
            Collider[] initialCollisions = Physics.OverlapSphere(transform.position, .1f, collisionMask);
            if (initialCollisions.Length > 0)
            {
                OnHitObject(initialCollisions[0]);
            }
        }

        protected virtual void Update()
        {
            float moveDistance = Speed * Time.deltaTime;
            CheckCollisions(moveDistance);
            transform.Translate(Vector3.forward * moveDistance);
        }

        private void CheckCollisions(float moveDistance)
        {
            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, moveDistance + skinWidth, collisionMask, QueryTriggerInteraction.Collide))
            {
                OnHitObject(hit);
            }
        }

        private void OnHitObject(RaycastHit hit)
        {
            base.CallPhysicContact(hit.collider.gameObject);
        }

        private void OnHitObject(Collider collider)
        {
            base.CallPhysicContact(collider.gameObject);
        }

        protected override void OnPhysicAttacking()
        {
            Destroy(gameObject);
        }

    }
}
