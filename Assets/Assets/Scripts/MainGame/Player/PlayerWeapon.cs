using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MainGame
{
    public class PlayerWeapon : MonoBehaviour
    {
        [SerializeField]
        private Transform weaponHolder;

        [SerializeField]
        private Gun defaultGun;

        private Gun equippedGun;

        protected virtual void Start()
        {
            if (defaultGun != null)
            {
                EquipGun(defaultGun);
            }
        }

        public void EquipGun(Gun newGun)
        {
            if (equippedGun != null)
            {
                Destroy(equippedGun);
            }
            equippedGun = Instantiate(newGun, weaponHolder.position, weaponHolder.rotation, weaponHolder) as Gun;
        }

        public void Shoot()
        {
            if (equippedGun != null)
            {
                equippedGun.Shoot();
            }
        }
    }
}
