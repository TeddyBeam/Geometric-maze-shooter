using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BaseSystems.Serializer;
using MainGame.Data;

namespace MainGame.Managers
{
    public class DataManager : MonoBehaviour
    {
        [SerializeField]
        private string saveFileName = "PlayerData.dat";

        private ISerializeHelper dataSerializer = new BinaryHelper();
        private PlayerData playerData;

        protected virtual void Awake()
        {
            //if (dataSerializer.TryLoad(saveFileName, out playerData))
            //{

            //}
            //else
            //{
            //    // TODO: first time play tutorial,...
            //}
        }
    }
}
