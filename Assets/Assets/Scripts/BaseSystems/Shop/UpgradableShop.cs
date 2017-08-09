using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BaseSystems.Data.Storage;
using BaseSystems.Observer;
using Extension.ExtraTypes;

namespace BaseSystems.Shops
{
    public class UpgradableShop : MonoBehaviour
    {
        #region Inner classes shop configuration
        [Serializable]
        public class UpgrabdableItem
        {
            public NamedObject objectInfo;
            public int price;
        }

        [Serializable]
        public class UpgradableItemShopInfo
        {
            [Reorderable(null, "Upgradable items: ", null)]
            public UpgrabdableItemList upgradableItemsInfo;
            public Image itemDisplay;
            public Button upgradeButton;
            public Text upgradePriceDisplay;
            public Button chooseButton;
        }

        [Serializable]
        public class UpgrabdableItemList : ReorderableArray<UpgrabdableItem> { }

        [Serializable]
        public class UpgradableItemShopInfoList: ReorderableArray<UpgradableItemShopInfo> { }
        #endregion

        #region Init
        [SerializeField]
        private Text playerMoneyDisplay = null;

        [SerializeField]
        private GameObject notEnoughMoneyDisplay;

        [SerializeField, Reorderable(null, "Upgradable items' info: ", null)]
        private UpgradableItemShopInfoList allItemsInfo = null;

        private Action<object> OnDataLoadedAction = null;
        private bool isQuitting = false;
        #endregion

        #region Monobehaviours
        protected virtual void Awake()
        {
            AddEventListener();
            /// hide not enough money message panel.
            notEnoughMoneyDisplay.SetActive(false);
        }

        protected virtual void Start()
        {
            /// request data from DataManager, will received back OnDataLoaded event.
            Debug.Log("ShopManager OnDataRequest event posted" + this.GetInstanceID());
            this.PostEvent(ObserverEventID.OnDataRequest);
        }

        protected virtual void OnApplicationQuit()
        {
            isQuitting = true;
        }

        protected virtual void OnDestroy()
        {
            if (!isQuitting)
            {
                RemoveEventListener();
            }
        }
        #endregion

        #region Add, remove events
        private void AddEventListener()
        {
            OnDataLoadedAction = (param) => SetupShop((PlayerData)param);
            this.RegisterListener(ObserverEventID.OnDataLoaded, OnDataLoadedAction);
            Debug.Log("ShopManager OnDataLoaded event listener added." + this.GetInstanceID());
        }

        private void RemoveEventListener()
        {
            this.RemoveListener(ObserverEventID.OnDataLoaded, OnDataLoadedAction);
            Debug.Log("ShopManager OnDataLoaded event listener removed" + this.GetInstanceID());
        }
        #endregion

        #region Event callback => setup shop, buy ship
        /// <summary>
        /// Will be invoked when OnDataLoaded be raised.
        /// </summary>
        /// <param name="data">Data sent from DataTransporter.</param>
        private void SetupShop(PlayerData data)
        {
            Debug.Assert(data != null, "SetupShop, null data.");
            // Debug.Log("ShopManager OnDataLoaded event received, Start shop setup." + this.GetInstanceID());

            playerMoneyDisplay.text = data.Money.ToString(); // Show player's money.
            if (allItemsInfo.Length > 0)
            {
                foreach (UpgradableItemShopInfo itemsInfo in allItemsInfo)
                {
                    for (int i = 0; i < itemsInfo.upgradableItemsInfo.Length; i++)
                    {
                        if (i != itemsInfo.upgradableItemsInfo.Length - 1) // If the item is not the final item.
                        {
                            /// Check if user hasnt has the next item.
                            if (!CheckOwnedItem(data, itemsInfo.upgradableItemsInfo[i + 1].objectInfo.NameID))
                            {
                                UpdateShopInfo(data, itemsInfo, i);
                                // show the next item price.
                                itemsInfo.upgradePriceDisplay.text = itemsInfo.upgradableItemsInfo[i + 1].price.ToString();
                                // upgrade button will upgrade into the next item.
                                itemsInfo.upgradeButton.onClick.AddListener(() => UpgradeButtonsEvent(data, itemsInfo, i + 1));
                                // go to next item's brand.
                                break;
                            }
                        }
                        else // reached the final item, that mean user already has all the items in this brand.
                        {
                            UpdateShopInfo(data, itemsInfo, i);
                            // disable the upgrade button.
                            itemsInfo.upgradePriceDisplay.text = "Max Level.";
                            itemsInfo.upgradeButton.interactable = false;
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("Empty shipsBuyInfo.");
            }
        }

        /// <summary>
        /// Will be added into the upgrade buttons.
        /// </summary>
        /// <param name="data">Data from DataTransporter.</param>
        /// <param name="itemsInfo">Current brand itemsInfo.</param>
        /// <param name="upgradeItemIndex">Index of the item user upgrade into when click the button.</param>
        public void UpgradeButtonsEvent(PlayerData data, UpgradableItemShopInfo itemsInfo, int upgradeItemIndex)
        {
            Debug.Assert(data != null, "BuyButtonsEvent, data is null");
            Debug.Assert(itemsInfo != null, "BuyButtonsEvent, itemShopInfo is null");
            Debug.Assert(upgradeItemIndex > 0, "BuyButtonsEvent, logic error.");

            if (itemsInfo.upgradableItemsInfo[upgradeItemIndex].price <= data.Money) // if player has enough money
            {
                data.Money -= itemsInfo.upgradableItemsInfo[upgradeItemIndex].price; // pay the price.
                data.OwnedItems.Add(itemsInfo.upgradableItemsInfo[upgradeItemIndex].objectInfo.NameID); // add the item into owned list.
                playerMoneyDisplay.text = data.Money.ToString(); // update player's money display.
                UpdateShopInfo(data, itemsInfo, upgradeItemIndex); // change current using item into this.
                // set the current item in the database into this item                                                           
                data.CurrentShip = itemsInfo.upgradableItemsInfo[upgradeItemIndex].objectInfo.NameID;

                /// Check if the item user has just bought is the final item.
                if (upgradeItemIndex + 1 < itemsInfo.upgradableItemsInfo.Length) // if not
                {
                    // update the upgrade button with the next item.
                    itemsInfo.upgradeButton.onClick.AddListener(() => UpgradeButtonsEvent(data, itemsInfo, upgradeItemIndex + 1));
                    // show the next upgrade price
                    itemsInfo.upgradePriceDisplay.text = itemsInfo.upgradableItemsInfo[upgradeItemIndex + 1].price.ToString();
                }
                else
                {
                    // nothing to upgrade anymore, disable upgrade button.
                    itemsInfo.upgradePriceDisplay.text = "Max Level.";
                    itemsInfo.upgradeButton.interactable = false;
                }
            }
            else
            {
                notEnoughMoneyDisplay.SetActive(true); // show not enough money message.
            }
        }
        #endregion

        #region Shop setup utilities
        /// <summary>
        /// Check if user has the item or not
        /// </summary>
        /// <param name="itemName">NameID of the item.</param>
        /// <param name="ownedList">List of the owned item from the data.</param>
        /// <returns></returns>
        private bool CheckOwnedItem(PlayerData data, NameIDs itemName)
        {
            Debug.Assert(itemName != NameIDs.None, "CheckOwnedShip, shipName is None.");
            Debug.Assert(data != null, "CheckOwnedShip, data is null.");

            if (data.OwnedItems.Contains(itemName))
                return true;
            return false;
        }

        /// <summary>
        /// Change the currently using item shop info into the next item.
        /// </summary>
        /// <param name="data">Data from DataTransporter.</param>
        /// <param name="shopInfo">Current brand itemsInfo.</param>
        /// <param name="currentItemIndex">Index of the currently using item.</param>
        private void UpdateShopInfo(PlayerData data, UpgradableItemShopInfo shopInfo, int currentItemIndex)
        {
            // Get the render sprite of the items and change the image display to this.
            shopInfo.itemDisplay.sprite = shopInfo.upgradableItemsInfo[currentItemIndex].objectInfo.SpawnPrefab.
                GetComponentInChildren<SpriteRenderer>().sprite;
            // Change the size of the image into the sprite size.
            shopInfo.itemDisplay.SetNativeSize();
            // choose button will choose this item.
            shopInfo.chooseButton.onClick.AddListener(() => { data.CurrentShip = shopInfo.upgradableItemsInfo[currentItemIndex].objectInfo.NameID; });
        }

        private void UpdateUpgradePriceDisplay(UpgradableItemShopInfo item, string message)
        {
            item.upgradePriceDisplay.text = message;
        }
        #endregion
    }
}
