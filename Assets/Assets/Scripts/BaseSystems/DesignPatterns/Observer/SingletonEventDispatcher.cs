using UnityEngine;
using System.Collections.Generic;
using System;

namespace BaseSystems.DesignPatterns.Observer
{
    public class SingletonEventDispatcher : EventDispatcher
    {
        private static volatile SingletonEventDispatcher instance;
        private static object synRoot = new object();

        public static SingletonEventDispatcher Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (synRoot)
                    {
                        if (instance == null)
                        {
                            instance = new SingletonEventDispatcher();
                        }
                    }
                }
                return instance;
            }
        }

        private SingletonEventDispatcher () { }
    }
}