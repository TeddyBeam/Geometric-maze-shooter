using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BaseSystems.DesignPatterns.Observer;
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
            SingletonEventDispatcher.Instance.RegisterListener(EventsID.OnGameOver, OnGameOverHandler);
        }

        protected virtual void OnApplicationQuit()
        {
            isQuitting = true;
        }

        protected virtual void OnDestroy()
        {
            if(!isQuitting)
                SingletonEventDispatcher.Instance.RemoveListener(EventsID.OnGameOver, OnGameOverHandler);
        }

        private void OnGameOverHandle()
        {
            gameOverPanel.SetActive(true);
        }
    }
}
