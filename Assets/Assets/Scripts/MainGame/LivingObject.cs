using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BaseSystems.PhysicLogic;
using System;

namespace MainGame
{
    public class LivingObject : MonoBehaviour, IAttackable<int>
    {
        [SerializeField, Range(1, 100)]
        private int startingHealth = 10;

        protected int currentHealth = 0;
        protected bool isDead = false;

        public event Action OnDeath;

        protected virtual void Start()
        {
            currentHealth = startingHealth;
        }

        public virtual void OnBeingAttacked(int damage, Vector3 hitPoint = default(Vector3), Vector3 hitDirection = default(Vector3))
        {
            currentHealth -= damage;
            if (currentHealth <= 0 && !isDead)
            {
                Die();
            }
        }

        protected virtual void Die()
        {
            isDead = true;            
            if(OnDeath != null)
            {
                OnDeath();
            }
            Destroy(gameObject);
        }
    }
}
