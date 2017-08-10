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
        private GameObject flashEffect;

        [SerializeField]
        private Projectile bullet;

        [SerializeField, Range(0.01f, 2f)]
        private float shotRate = 0.1f;

        [SerializeField, Range(10f, 100f)]
        private float muzzleVelocity = 35f;

        [Header("Recoil setup.")]
        [SerializeField, Range(10f, 100f)]
        private float kickBackAngle = 20f;

        [SerializeField, Range(0.1f, 1f)]
        private float kickBackForce = 0.2f, recoilSmoothTime = 0.1f;

        private Vector3 recoilVelocity = Vector3.zero;
        private float recoilAngle;
        private float recoilAngleSmoothVelocity;
        private float nextShotTime;

        protected virtual void FixedUpdate()
        {
            /// Recoil animation (Slowly go back to normal position)
            transform.localPosition = Vector3.SmoothDamp(transform.localPosition, Vector3.zero, ref recoilVelocity, recoilSmoothTime);
            recoilAngle = Mathf.SmoothDamp(recoilAngle, 0, ref recoilAngleSmoothVelocity, recoilSmoothTime);
            transform.localEulerAngles = Vector3.left * recoilAngle;
        }

        public void Shoot()
        {
            if (Time.time > nextShotTime)
            {
                nextShotTime = Time.time + shotRate;
                Projectile newProjectile = Instantiate(bullet, muzzle.position, muzzle.rotation) as Projectile;
                newProjectile.Speed = muzzleVelocity;
                flashEffect.SetActive(true);

                /// Recoil animation (Apply force)
                transform.localPosition -= Vector3.forward * kickBackForce;
                recoilAngle += kickBackAngle;
                recoilAngle = Mathf.Clamp(recoilAngle, 0, 30);
            }
        }
    }
}
