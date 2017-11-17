using Extension.ExtraTypes;
using MainGame.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MainGame.Shop
{
    public class ShopManager : MonoBehaviour
    {
        [SerializeField, Reorderable]
        private GunDataList guns;
    }
}
