using UnityEngine;

namespace BaseSystems.Data.Storage
{   
    /// <summary>
    /// Give a game object an nameID to spawn, use object pooler, save....
    /// </summary>
    [CreateAssetMenu(fileName = "Named Object", menuName = "Scriptable Assets/Named Object Config", order = 0)]
    public class NamedObject : ScriptableObject
    {
        [SerializeField]
        private GameObject spawnPrefab;

        [SerializeField]
        private NameIDs nameID;

        public GameObject SpawnPrefab { get { return spawnPrefab; } set { spawnPrefab = value; } }
        public NameIDs NameID { get { return nameID; } set { nameID = value; } }

        /// <param name="prefab">Prefab of this game object.</param>
        /// <param name="name">Name to identify this game object.</param>
        public NamedObject(GameObject prefab, NameIDs name)
        {
            spawnPrefab = prefab;
            nameID = name;
        }
    }
}