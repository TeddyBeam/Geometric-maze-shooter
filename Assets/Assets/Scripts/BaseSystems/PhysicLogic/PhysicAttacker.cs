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

        protected void CallPhysicContact(GameObject victimObject)
        {
            IEnumerable<IPhysicVictim<T>> victimPhysicInterfaces = victimObject.GetComponents<IPhysicVictim<T>>();
            if (victimPhysicInterfaces != null)
            {
                foreach (IPhysicVictim<T> physicInterface in victimPhysicInterfaces)
                {
                    physicInterface.OnPhysicAttacked(physicParam);
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
