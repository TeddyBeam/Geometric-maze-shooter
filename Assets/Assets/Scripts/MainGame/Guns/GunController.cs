using UnityEngine;
using Extension.ExtraTypes;

namespace MainGame.Guns
{
    public class GunController : MonoBehaviour, IShootable
    {
        [SerializeField]
        private Transform weaponHolder;

        [SerializeField, Reorderable]
        private GunList guns;

        private Gun equippedGun;

        protected virtual void Start()
        {
            if (guns != null)
            {
                EquipGun(guns[0]);
            }
            else
            {
                Debug.Log("Null defaultGun");
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
            else
            {
                Debug.Log("Null equippedGun");
            }
        }
    }
}
