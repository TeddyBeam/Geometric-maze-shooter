using System;

namespace BaseSystems.DesignPatterns.Zenject
{
    public class ValidationMarker
    {
        public ValidationMarker(Type markedType)
        {
            MarkedType = markedType;
        }

        public Type MarkedType
        {
            get;
            private set;
        }
    }
}

