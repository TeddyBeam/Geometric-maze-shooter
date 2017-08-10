using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BaseSystems.PhysicLogic
{
    /// <summary>
    /// All class implement this interface can be attacked by
    /// object that has PhysicAttacker component.
    /// </summary>
    /// <typeparam name="T">Type of the physic param the will be received from attacker.</typeparam>
    public interface IAttackable<T>
    {
        /// <summary>
        /// Will be called on the victim when its being attacked.
        /// </summary>
        /// <param name="param">Parameter sent from attacker.</param>
        /// <param name="hitPoint">Where the attack hit.</param>
        /// <param name="attackDirection">Direction of the attack.</param>
        void OnBeingAttacked(T param, Vector3 hitPoint = default(Vector3), Vector3 attackDirection = default(Vector3));
    }
}
