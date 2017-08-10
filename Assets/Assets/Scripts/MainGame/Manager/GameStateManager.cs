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

        protected virtual void Start()
        {
            OnGameOverHandler = _ => OnGameOverHandle();
            this.RegisterListener(ObserverEventID.OnGameOver, OnGameOverHandler);
        }

        protected virtual void OnDestroy()
        {
            this.RemoveListener(ObserverEventID.OnGameOver, OnGameOverHandler);
        }

        private void OnGameOverHandle()
        {
            gameOverPanel.SetActive(true);
        }
    }
}
