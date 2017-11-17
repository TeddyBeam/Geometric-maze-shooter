using System.Collections.Generic;

namespace BaseSystems.DesignPatterns.Zenject
{
    public interface ISubContainerCreator
    {
        DiContainer CreateSubContainer(List<TypeValuePair> args, InjectContext context);
    }
}
