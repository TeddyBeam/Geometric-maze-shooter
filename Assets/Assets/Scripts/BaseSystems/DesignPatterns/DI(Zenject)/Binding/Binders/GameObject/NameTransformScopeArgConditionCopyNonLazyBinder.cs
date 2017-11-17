#if !NOT_UNITY3D

namespace BaseSystems.DesignPatterns.Zenject
{
    public class NameTransformScopeArgConditionCopyNonLazyBinder : TransformScopeArgConditionCopyNonLazyBinder
    {
        public NameTransformScopeArgConditionCopyNonLazyBinder(
            BindInfo bindInfo,
            GameObjectCreationParameters gameObjectInfo)
            : base(bindInfo, gameObjectInfo)
        {
        }

        public TransformScopeArgConditionCopyNonLazyBinder WithGameObjectName(string gameObjectName)
        {
            GameObjectInfo.Name = gameObjectName;
            return this;
        }
    }
}

#endif
