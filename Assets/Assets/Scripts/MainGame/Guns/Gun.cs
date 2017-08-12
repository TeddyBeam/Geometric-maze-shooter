using System;
using System.Collections.Generic;
using UnityEngine;
using Extension.Attributes;
using Extension.ExtraTypes;
using MainGame.Managers;
using MainGame.Bullets;

namespace MainGame.Guns
{
    [Serializable]
    public class GunList : ReorderableArray<Gun> { }

    public class Gun : MonoBehaviour, IShootable
    {
        [SerializeField]
        private Transform muzzle;

        [SerializeField]
        private GameObject flashEffect;

        [SerializeField]
        private Projectile bullet;

        [SerializeField]
        private AudioClip fireAudio;

        [SerializeField, Range(0.01f, 2f)]
        private float shotRate = 0.1f;

        [SerializeField, Range(10f, 100f)]
        private float muzzleVelocity = 35f;

        [Header("Recoil setup.")]
        [SerializeField, MinMaxSlider(5f, 50f)]
        private Vector2 kickBackAngle = new Vector2(10f, 20f);

        [SerializeField, MinMaxSlider(0.01f, 1f), Space(15)]
        private Vector2 kickBackForce = new Vector2(0.1f, 0.2f);

        [SerializeField, Range(0.1f, 1f), Space(15)]
        private float recoilSmoothTime = 0.1f;

        private Vector3 recoilVelocity = Vector3.zero;
        private float recoilAngle, recoilAngleSmoothVelocity, nextShotTime;

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
                AudioManager.Instance.PlaySfx(fireAudio, transform.position);

                /// Recoil animation (Apply force)
                transform.localPosition -= Vector3.forward * UnityEngine.Random.Range(kickBackForce.x, kickBackForce.y);
                recoilAngle += UnityEngine.Random.Range(kickBackAngle.x, kickBackAngle.y);
                recoilAngle = Mathf.Clamp(recoilAngle, 0, 30);
            }
        }
    }
}
