using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MainGame.Managers
{
    public class ApplicationStateManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject pausePanel;

        private void OnApplicationFocus(bool focus)
        {
            if (!focus)
            {
                Pause();
            }
            else
            {
                Continue();
            }
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                Pause();
            }
            else
            {
                Continue();
            }
        }

        public void Pause()
        {
            pausePanel.SetActive(true);
            Time.timeScale = 0.0f;
        }

        public void Continue()
        {
            pausePanel.SetActive(false);
            Time.timeScale = 1.0f;
        }
    }
}
