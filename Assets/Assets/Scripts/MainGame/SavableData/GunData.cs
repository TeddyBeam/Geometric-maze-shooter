using System;
using UnityEngine;
using Extension.ExtraTypes;
using MainGame.Bullets;

namespace MainGame.Data
{
    [Serializable]
    public class GunDataList : ReorderableArray<GunData> { }

    [Serializable]
    [CreateAssetMenu(fileName = "Gun Data", menuName = "Scriptable Assets/Gun Data", order = 1)]
    public class GunData : ScriptableObject
    {
        public GunsName gunName;
        
        public Projectile bullet;

        public AudioClip fireAudio;

        [Range(1, 10)]
        public int upgradeLevel;

        [Range(0.01f, 2f)]
        public float shotRate = 0.1f;

        [Range(10f, 100f)]
        public float muzzleVelocity = 35f;

        [Header("Recoil setup.")]
        // [MinMaxSlider(5f, 50f)]
        public Vector2 kickBackAngle = new Vector2(10f, 20f);

        // [MinMaxSlider(0.01f, 1f), Space(15)]
        public Vector2 kickBackForce = new Vector2(0.1f, 0.2f);

        [Range(0.1f, 1f), Space(15)]
        public float recoilSmoothTime = 0.1f;
    }
}
