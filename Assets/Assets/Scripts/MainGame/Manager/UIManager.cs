using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MainGame.Managers
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField]
        private Slider masterVolumeSlider, sfxVolumeSlider, musicVolumeSlider;

        protected virtual void Start()
        {

            /// Add sliders' callback events.
            masterVolumeSlider.onValueChanged.AddListener(_ => AudioManager.Instance.MasterVolunePercent = masterVolumeSlider.value);
            sfxVolumeSlider.onValueChanged.AddListener(_ => AudioManager.Instance.SfxVolumePercent = sfxVolumeSlider.value);
            musicVolumeSlider.onValueChanged.AddListener(_ => AudioManager.Instance.MusicVolumePercent = musicVolumeSlider.value);

            /// Set sliders' default display value.
            masterVolumeSlider.value = AudioManager.Instance.masterVolumePercent;
            sfxVolumeSlider.value = AudioManager.Instance.sfxVolumePercent;
            musicVolumeSlider.value = AudioManager.Instance.musicVolumePercent;
        }
    }
}
