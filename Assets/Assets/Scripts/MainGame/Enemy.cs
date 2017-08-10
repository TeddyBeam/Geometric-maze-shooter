using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Extension.Attributes;

namespace MainGame
{
    public class Enemy : LivingObject
    {
        #region Init
        [SerializeField, TagSelector]
        private string playerTag = "Player";

        [SerializeField]
        private GameObject deathEffect;

        [SerializeField, Range(1, 10)]
        private int attackDamage = 1;

        [SerializeField, Range(0.1f, 2f)]
        private float pathRefeshRate = 0.5f;

        [SerializeField, Range(0.5f, 5f)]
        private float attackDistance = 1.5f, attackSpeed = 3f, timeBetweenAttacks = 1f;

        [SerializeField, Range(0.01f, 10f)]
        private float myCollisionRadius = 0.1f, targetCollisionRadius = 0.5f;

        [SerializeField]
        private NavMeshAgent pathFinder;

        [SerializeField]
        private Renderer meshRenderer;

        public enum State { Idle, Chasing, Attacking };
        private State currentState;
        private LivingObject livingTarget;
        private Transform target;
        private Color originalColour;
        private Material skinMaterial;
        private float nextAttackTime = 0f;
        private bool hasTarget = false;
        #endregion

        #region Monobehaviour
        protected override void Start()
        {
            base.Start();

            currentState = State.Chasing;   

            skinMaterial = meshRenderer.material;
            originalColour = skinMaterial.color;

            /// Find player
            GameObject targetObj = GameObject.FindGameObjectWithTag(playerTag);
            if (targetObj != null)
            {
                hasTarget = true;
                target = targetObj.transform;
                livingTarget = target.GetComponent<LivingObject>();
                if(livingTarget != null)
                {
                    livingTarget.OnDeath += OnTargetDeath;
                }
                else
                {
                    Debug.LogError("There is no LivingObject component in " + targetObj.name);
                }
                StartCoroutine(UpdatePath());
            }
            else
            {
                Debug.LogWarning("Can't find any game object with tag: " + playerTag);
            }
        }

        protected virtual void FixedUpdate()
        {
            if (hasTarget && Time.time > nextAttackTime)
            {
                float sqrDstToTarget = (target.position - transform.position).sqrMagnitude;
                if (sqrDstToTarget < Mathf.Pow(attackDistance + myCollisionRadius + targetCollisionRadius, 2))
                {
                    nextAttackTime = Time.time + timeBetweenAttacks;
                    StartCoroutine(Attack());
                }
            }
        }
        #endregion

        #region Behaviours
        private IEnumerator Attack()
        {
            currentState = State.Attacking;
            pathFinder.enabled = false;

            Vector3 originalPosition = transform.position;
            Vector3 dirToTarget = (target.position - transform.position).normalized;
            Vector3 attackPosition = target.position - dirToTarget * (myCollisionRadius);

            float percent = 0;

            skinMaterial.color = Color.red;
            bool hasAppliedDamage = false;

            while (percent <= 1)
            {
                /// Attack the player when near enough
                if(percent > 0.5f && !hasAppliedDamage)
                {
                    hasAppliedDamage = true;
                    livingTarget.OnBeingAttacked(attackDamage);
                }

                /// Do an attack 
                percent += Time.deltaTime * attackSpeed;
                float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4;
                transform.position = Vector3.Lerp(originalPosition, attackPosition, interpolation);

                yield return null;
            }

            skinMaterial.color = originalColour;
            currentState = State.Chasing;
            pathFinder.enabled = true;
        }

        private IEnumerator UpdatePath()
        {
            while(hasTarget)
            {
                if (currentState == State.Chasing)
                {
                    Vector3 dirToTarget = (target.position - transform.position).normalized;
                    Vector3 targetPosition = target.position - dirToTarget * (myCollisionRadius + targetCollisionRadius + attackDistance / 2);
                    // Vector3 targetPosition = new Vector3(target.position.x, 0f, target.position.z);
                    if (!isDead)
                    {
                        pathFinder.SetDestination(targetPosition);
                    }
                }
                yield return new WaitForSeconds(pathRefeshRate);
            }
        }

        protected override void Die()
        {

            base.Die();
        }

        private void OnTargetDeath()
        {
            hasTarget = false;
            currentState = State.Idle;
        }

        public override void OnBeingAttacked(int damage, Vector3 hitPoint = default(Vector3), Vector3 hitDirection = default(Vector3))
        {
            if(damage > currentHealth)
            {
                Instantiate(deathEffect, hitPoint, Quaternion.FromToRotation(Vector3.forward, hitDirection));
            }
            base.OnBeingAttacked(damage, hitPoint, hitDirection);
        }
        #endregion
    }
}
