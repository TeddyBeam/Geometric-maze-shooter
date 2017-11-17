#if !NOT_UNITY3D

using ModestTree;

namespace BaseSystems.DesignPatterns.Zenject
{
    public class PrefabProvider : IPrefabProvider
    {
        readonly UnityEngine.Object _prefab;

        public PrefabProvider(UnityEngine.Object prefab)
        {
            Assert.IsNotNull(prefab);
            _prefab = prefab;
        }

        public UnityEngine.Object GetPrefab()
        {
            return _prefab;
        }
    }
}

#endif


