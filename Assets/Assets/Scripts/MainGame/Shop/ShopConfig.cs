using System;
using UnityEngine;
using UnityEngine.UI;
using MainGame.Data;
using Extension.ExtraTypes;

namespace MainGame.Shop
{
    [Serializable]
    public class ShopConfigList : ReorderableArray<ShopConfig> { }

    [Serializable]
    public class ShopConfig
    {
        public GunsName gunName;
        public int price;
        public Button buyButton;
        public GunDataList upgradeData;
    }
}
