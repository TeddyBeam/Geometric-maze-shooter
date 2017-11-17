using System;

namespace BaseSystems.DesignPatterns.Zenject
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ValidateOnlyAttribute : Attribute
    {
    }
}


