using System;
using System.Collections.Generic;
using UnityEngine;
using BaseSystems.Singleton;
using BaseSystems.Data.Storage;
using Extension.ExtraTypes;
namespace BaseSystems.ObjectPooling
{
    [Serializable]
    public class PooledObjectList : ReorderableArray<PooledObject> { }

    public class ObjectPooler: Singleton<ObjectPooler>
    {
        #region Init
        /// <summary>
        /// Setup default pooled objects.
        /// </summary>
        [SerializeField, Reorderable("namedObjectInfo")]
        protected PooledObjectList poolObjectDefaultList = null;

        /// <summary>
        /// Store all pooled objects.
        /// </summary>
        protected Dictionary<NameIDs, List<GameObject>> pooledObjectsDict = new Dictionary<NameIDs, List<GameObject>>();
        #endregion

        #region Get pooled object
        /// <summary>
        /// Get object out of the pool to use.
        /// Return one pooled object if its available, otherwise return null.
        /// </summary>
        /// <param name="nameID">NameID of the PooledItemInfo</param>
        public GameObject GetPooledObject(NameIDs nameID)
        {
            Debug.Assert(nameID != NameIDs.None, "GetPooledObject, nameID cant be None.");

            List<GameObject> tempList;
            foreach (PooledObject itemInfo in poolObjectDefaultList)
            {
                if (pooledObjectsDict.TryGetValue(nameID, out tempList)) // Check if the name key already in the pool.
                {
                    foreach (GameObject item in tempList)
                    {
                        if (!item.activeInHierarchy)
                            return item;
                    }
                    if (itemInfo.ShouldGrow)
                    {
                        GameObject expandObject = Instantiate(itemInfo.SpawnPrefab, transform) as GameObject;
                        expandObject.SetActive(false);
                        pooledObjectsDict[nameID].Add(expandObject);
                        Debug.Log("created new " + expandObject.name + " in the pool.");
                        return expandObject;
                    }
                }
            }
            // DebugUtil.Log("Coundn't find any {0} in the pool.", objectNameID);
            return null;
        }

        /// <summary>
        /// Add new game object into the pool.
        /// </summary>
        public void AddNewObject (PooledObject objectInfo)
        {
            CheckAndCreate(objectInfo);
        }
        #endregion

        #region Initialize pool, change scene clean , clean pool
        /// <summary>
        /// Will be invoked when OnFirstTimePlay be raised.
        /// Instantiate all pooled objects in 'pooledObjectsList' right when the pool is created.
        /// </summary>
        protected void InitPool()
        {
            foreach (PooledObject objectInfo in poolObjectDefaultList)
            {
                CheckAndCreate(objectInfo);
            }
        }

        /// <summary>
        /// Destroy all objects that dont have shouldKeep checked.
        /// </summary>
        protected void HandleUnnecessaryObjects()
        {
            foreach (PooledObject objectInfo in poolObjectDefaultList)
            {
                List<GameObject> tempList;
                if (pooledObjectsDict.TryGetValue(objectInfo.NameID, out tempList))
                {
                    if (objectInfo.ShouldKeep)
                    {
                        foreach (GameObject poolObject in tempList)
                        {
                            if (poolObject != null)
                                poolObject.SetActive(false);
                        }
                    }
                    else
                    {
                        foreach (GameObject poolObject in tempList)
                        {
                            if (poolObject != null)
                                Destroy(poolObject);
                        }
                    }
                }
                // Debug.Log("All unnescessatry objects in the pool have been handled. " + this.GetInstanceID());   
            }
        }

        protected void CleanPool ()
        {
            pooledObjectsDict.Clear();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Check if the pooledObjectsDict already has the name key or not, then instantiate.
        /// </summary>
        /// <param name="objectInfo"></param>
        private void CheckAndCreate(PooledObject objectInfo)
        {
            List<GameObject> tempList = null;
            if (pooledObjectsDict.TryGetValue(objectInfo.NameID, out tempList))
            {
                InstantiateAndAdd(objectInfo, tempList);
            }
            else // create a new list and add it into the dict.
            {
                List<GameObject> objList = new List<GameObject>();
                InstantiateAndAdd(objectInfo, objList);
                pooledObjectsDict.Add(objectInfo.NameID, objList);
            }
        }

        /// <summary>
        /// Instantiate all objects and add them into the list of the pool dic.
        /// </summary>
        private void InstantiateAndAdd(PooledObject itemInfo, List<GameObject> objList)
        {
            for (int i = 0; i < itemInfo.DefaultAmount; i++)
            {
                GameObject pooledObject = Instantiate(itemInfo.SpawnPrefab, transform) as GameObject;
                pooledObject.SetActive(false);
                objList.Add(pooledObject);
                // PutIntoPoolTransform(pooledObject);
            }
        }
        #endregion
    }
}