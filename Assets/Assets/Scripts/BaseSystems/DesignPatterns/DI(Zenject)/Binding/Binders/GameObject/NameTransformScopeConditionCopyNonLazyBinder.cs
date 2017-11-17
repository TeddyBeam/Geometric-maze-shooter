#if !NOT_UNITY3D

namespace BaseSystems.DesignPatterns.Zenject
{
    public class NameTransformScopeConditionCopyNonLazyBinder : TransformScopeConditionCopyNonLazyBinder
    {
        public NameTransformScopeConditionCopyNonLazyBinder(
            BindInfo bindInfo,
            GameObjectCreationParameters gameObjectInfo)
            : base(bindInfo, gameObjectInfo)
        {
        }

        public TransformScopeConditionCopyNonLazyBinder WithGameObjectName(string gameObjectName)
        {
            GameObjectInfo.Name = gameObjectName;
            return this;
        }
    }
}

#endif
