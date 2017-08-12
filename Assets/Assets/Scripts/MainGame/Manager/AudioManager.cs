using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BaseSystems.Singleton;

namespace MainGame.Managers
{
    public class AudioManager : Singleton<AudioManager>
    {
        private AudioSource audioSource;

        [SerializeField, Range(0.01f, 1f)]
        public float masterVolumePercent = 1f, sfxVolumePercent = 1f, musicVolumePercent = 1f;

        public float MasterVolunePercent
        {
            get
            {
                return masterVolumePercent;
            }
            set
            {
                masterVolumePercent = value;
                PlayerPrefs.SetFloat(masterVolumeSaveName, masterVolumePercent);
            }
        }

        public float SfxVolumePercent
        {
            get
            {
                return sfxVolumePercent;
            }
            set
            {
                sfxVolumePercent = value;
                PlayerPrefs.SetFloat(sfxVolumeSaveName, sfxVolumePercent);
            }
        }

        public float MusicVolumePercent
        {
            get
            {
                return musicVolumePercent;
            }
            set
            {
                musicVolumePercent = value;
                PlayerPrefs.SetFloat(musicVolumeSaveName, musicVolumePercent);
            }
        }

        [SerializeField]
        private string masterVolumeSaveName = "master", sfxVolumeSaveName = "sfx", musicVolumeSaveName = "music";

        protected override void Awake()
        {
            base.Awake();

            /// Load audios' volume setting.
            MasterVolunePercent = PlayerPrefs.GetFloat(masterVolumeSaveName, 1f);
            SfxVolumePercent = PlayerPrefs.GetFloat(sfxVolumeSaveName, 1f);
            MusicVolumePercent = PlayerPrefs.GetFloat(musicVolumeSaveName, 1f);

            /// Add an AudioSource.
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        public void PlaySfx(AudioClip sound, Vector3 position)
        {
            Debug.Assert(sound != null, "Null " + sound);
            AudioSource.PlayClipAtPoint(sound, position, sfxVolumePercent * masterVolumePercent);
        }

        public void PlaySfx(AudioClip sound)
        {
            Debug.Assert(sound != null, "Null " + sound);
            audioSource.PlayOneShot(sound, sfxVolumePercent * masterVolumePercent);
        }
    }
}
