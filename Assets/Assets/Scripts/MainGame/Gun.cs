using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MainGame
{
    public class Gun : MonoBehaviour
    {
        [SerializeField]
        private Transform muzzle;

        [SerializeField]
        private Projectile bullet;

        [SerializeField, Range(0.01f, 2f)]
        private float shotRate = 0.1f;

        [SerializeField, Range(10f, 100f)]
        private float muzzleVelocity = 35f;

        private float nextShotTime;

        public void Shoot()
        {
            if (Time.time > nextShotTime)
            {
                nextShotTime = Time.time + shotRate;
                Projectile newProjectile = Instantiate(bullet, muzzle.position, muzzle.rotation) as Projectile;
                newProjectile.Speed = muzzleVelocity;
            }
        }
    }
}
