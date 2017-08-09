using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BaseSystems.PhysicLogic;
using System;

namespace MainGame
{
    public class LivingObject : MonoBehaviour, IPhysicVictim<int>
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

        public virtual void OnPhysicAttacked(int damage)
        {
            currentHealth -= damage;
            if(currentHealth <= 0 && !isDead)
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
