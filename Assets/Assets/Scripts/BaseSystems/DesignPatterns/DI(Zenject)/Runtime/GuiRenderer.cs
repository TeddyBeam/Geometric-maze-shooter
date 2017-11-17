using System;
using System.Collections.Generic;
using System.Linq;
using ModestTree;
using ModestTree.Util;
using UnityEngine;

namespace BaseSystems.DesignPatterns.Zenject
{
    public class GuiRenderer : MonoBehaviour
    {
        GuiRenderableManager _renderableManager;

        [Inject]
        void Construct(GuiRenderableManager renderableManager)
        {
            _renderableManager = renderableManager;
        }

        public void OnGUI()
        {
            _renderableManager.OnGui();
        }
    }
}
