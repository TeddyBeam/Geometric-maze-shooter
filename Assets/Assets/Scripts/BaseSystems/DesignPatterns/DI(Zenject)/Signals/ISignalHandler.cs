using System;
using System.Collections.Generic;
using ModestTree;

namespace BaseSystems.DesignPatterns.Zenject
{
    public interface ISignalHandler
    {
        void Execute(object[] args);
    }
}
