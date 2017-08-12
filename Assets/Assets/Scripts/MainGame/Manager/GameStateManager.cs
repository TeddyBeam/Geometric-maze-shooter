using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BaseSystems.Observer;
using System;

namespace MainGame.Managers
{
    public class GameStateManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject gameOverPanel;

        private Action<object> OnGameOverHandler;
        private bool isQuitting = false;

        protected virtual void Start()
        {
            OnGameOverHandler = _ => OnGameOverHandle();
            this.RegisterListener(ObserverEventID.OnGameOver, OnGameOverHandler);
        }

        protected virtual void OnApplicationQuit()
        {
            isQuitting = true;
        }

        protected virtual void OnDestroy()
        {
            if(!isQuitting)
                this.RemoveListener(ObserverEventID.OnGameOver, OnGameOverHandler);
        }

        private void OnGameOverHandle()
        {
            gameOverPanel.SetActive(true);
        }
    }
}
