using System;
using System.Collections.Generic;
using UnityEngine;

namespace BaseSystems.PhysicLogic
{
    /// <summary>
    /// Attacker can attack all object that has implemented IPhysicVictim.
    /// </summary>
    /// <typeparam name="T">Type of the physic parameter that will be sent to victim.</typeparam>
    public abstract class PhysicAttacker<T> : MonoBehaviour
    {
        [SerializeField]
        private T physicParam = default(T);

        protected void Attack(GameObject victimObject, Vector3 hitPoint = default(Vector3), Vector3 attackDirection = default(Vector3))
        {
            IEnumerable<IAttackable<T>> victimPhysicInterfaces = victimObject.GetComponents<IAttackable<T>>();
            if (victimPhysicInterfaces != null)
            {
                foreach (IAttackable<T> physicInterface in victimPhysicInterfaces)
                {
                    physicInterface.OnBeingAttacked(physicParam, hitPoint, attackDirection);
                }
                OnPhysicAttacking();
            }
        }

        protected void Hit(GameObject victimObject)
        {
            IEnumerable<IAttackable<T>> victimPhysicInterfaces = victimObject.GetComponents<IAttackable<T>>();
            if (victimPhysicInterfaces != null)
            {
                foreach (IAttackable<T> physicInterface in victimPhysicInterfaces)
                {
                    physicInterface.OnBeingAttacked(physicParam);
                }
                OnPhysicAttacking();
            }
        }

        /// <summary>
        /// Will be invoke when game object is attacking a victim.
        /// </summary>
        protected abstract void OnPhysicAttacking();
    }
}
