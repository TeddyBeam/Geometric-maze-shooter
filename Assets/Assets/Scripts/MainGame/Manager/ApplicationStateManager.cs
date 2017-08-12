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
                PauseGame();
            }
            else
            {
                ContinueGame();
            }
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                PauseGame();
            }
            else
            {
                ContinueGame();
            }
        }

        public void PauseGame()
        {
            pausePanel.SetActive(true);
            Time.timeScale = 0.0f;
        }

        public void ContinueGame()
        {
            pausePanel.SetActive(false);
            Time.timeScale = 1.0f;
        }
    }
}
