using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BaseSystems
{
    namespace PhysicLogic
    {
        /// <summary>
        /// All class implement this interface can be attacked by
        /// object that has PhysicAttacker component.
        /// </summary>
        /// <typeparam name="T">Type of the physic param the will be received from attacker.</typeparam>
        public interface IPhysicVictim<T>
        {
            /// <summary>
            /// Will be called on the victim when its being attacked.
            /// </summary>
            /// <param name="param">Parameter sent from attacker.</param>
            void OnPhysicAttacked(T param);
        }
    }
}
