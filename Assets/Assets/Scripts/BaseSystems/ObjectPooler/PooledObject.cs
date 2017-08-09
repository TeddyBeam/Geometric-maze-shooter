using System;
using UnityEngine;
using Extension.Attributes;
using Extension.ExtraTypes;
using BaseSystems.Data.Storage;

namespace BaseSystems.ObjectPooling
{
    [Serializable]
    public class PooledObject
    {
        [SerializeField]
        private NamedObject namedObjectInfo;

        [SerializeField, Positive]
        private int defaultAmount;

        [SerializeField, Comment("Instantiate new object if reached defaultAmount?")]
        private bool shouldGrow;

        /// <summary>
        /// Is this object DontDestroyOnLoad ?
        /// </summary>
        [SerializeField, Comment("Keep this object between scenes?")]
        private bool shouldKeep;

        public NameIDs NameID { get { return namedObjectInfo.NameID; } private set { namedObjectInfo.NameID = value; } }
        public GameObject SpawnPrefab { get { return namedObjectInfo.SpawnPrefab; } private set { namedObjectInfo.SpawnPrefab = value; } }
        public int DefaultAmount { get { return defaultAmount; } }
        public bool ShouldGrow { get { return shouldGrow; } }
        public bool ShouldKeep { get { return shouldKeep; } }

        /// <param name="prefab">Prefab of this game object.</param>
        /// <param name="name">Name to identify this game object.</param>
        /// <param name="amount">Default amount this game object will be instantiated.</param>
        /// <param name="grow">Instantiate a new game object if amount limit reached?</param>
        /// <param name="keep">Keep this game object after HandleUnnecessaryObjects()?</param>
        public PooledObject(GameObject prefab, NameIDs name, int amount, bool grow, bool keep)
        {
            namedObjectInfo.NameID = name;
            namedObjectInfo.SpawnPrefab = prefab;
            defaultAmount = amount;
            shouldGrow = grow;
            shouldKeep = keep;
        }

        /// <param name="namedObject">Named object info.</param>
        /// <param name="amount">Default amount this game object will be instantiated.</param>
        /// <param name="grow">Instantiate a new game object if amount limit reached?</param>
        /// <param name="keep">Keep this game object after HandleUnnecessaryObjects()?</param>
        public PooledObject(NamedObject namedObject, int amount, bool grow, bool keep)
        {
            namedObjectInfo = namedObject;
            defaultAmount = amount;
            shouldGrow = grow;
            shouldKeep = keep;
        }
    }
}
