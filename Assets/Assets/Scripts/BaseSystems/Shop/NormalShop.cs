using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BaseSystems.Data.Storage;
using BaseSystems.Observer;
using Extension.Attributes;

namespace BaseSystems.Shops
{
    #region Shop configuration
    [Serializable]
    public class NormalShopItemsInfo
    {
        public NameIDs nameID;
        public Sprite ownedSprite;
        public Sprite unownedSprite;
        public Button buyButton;
        public Image displayImage;
        public Text priceDisplay;
        public int price;
    }
    #endregion

    public class NormalShop : MonoBehaviour
    {
        #region Init
        [SerializeField, LabelField("nameID")]
        private NormalShopItemsInfo[] allShipsInfo = null;

        [SerializeField]
        private Text moneyDislay = null;

        [SerializeField]
        private GameObject notEnoughMoneyDisplay;

        private Action<object> OnDataLoadedAction = null;
        private NormalShopItemsInfo currentShopInfo;
        private bool isQuitting = false;
        #endregion

        #region Monobehaviours
        protected virtual void Awake ()
        {
            AddEventListener();
            /// hide not enough money message panel.
            notEnoughMoneyDisplay.SetActive(false);
        }
        protected virtual void Start ()
        {
            /// request data from DataManager, will received back OnDataLoaded event.
            Debug.Log("ShopManager OnDataRequest event posted" + this.GetInstanceID());
            this.PostEvent(ObserverEventID.OnDataRequest);        
        }

        protected virtual void OnApplicationQuit ()
        {
            isQuitting = true;
        }

        protected virtual void OnDestroy ()
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
        private void SetupShop (PlayerData data)
        {
            Debug.Assert(data != null, "SetupShop, null data.");
            // Debug.Log("ShopManager OnDataLoaded event received, Start shop setup." + this.GetInstanceID());
            UpdateMoneyDislay(data.Money.ToString());
            if (allShipsInfo.Length > 0)
            {
                foreach (NormalShopItemsInfo shipInfo in allShipsInfo)
                {
                    /// setup shop default display.
                    if (CheckOwnedShip(shipInfo.nameID, data.OwnedItems)) // if the ship is owned.
                    {
                        ChangeDisplayImage(shipInfo, true);
                        if (shipInfo.nameID == data.CurrentShip) // if this ship is same as current ship.
                        {
                            // using ship display
                            UpdateStatusDisplay(shipInfo, "Current.");
                            // SetCurrentShip(data, shipInfo.nameID);
                            currentShopInfo = shipInfo; // save current ship info for later use.
                        }
                        else
                        {
                            UpdateStatusDisplay(shipInfo, "Set");
                        }
                        // Debug.Log(shipInfo.buyButton + "marked as owned ship.");
                    }
                    else // if the ship is unowned.
                    {
                        ChangeDisplayImage(shipInfo, false);
                        UpdateStatusDisplay(shipInfo, shipInfo.price.ToString());
                        // Debug.Log(shipInfo.buyButton + "added as unowned ship.");
                    }

                    // add buy event into the button.
                    shipInfo.buyButton.onClick.AddListener(() => BuyButtonsEvent(data, shipInfo));
                }
            }
            else
            {
                Debug.Log("Empty shipsBuyInfo.");
            }
        }

        /// <summary>
        /// Will be added into the buy button of each ship.
        /// </summary>
        /// <param name="data">Data from DataTransporter.</param>
        /// <param name="shipInfo">The info of each ship.</param>
        public void BuyButtonsEvent (PlayerData data, NormalShopItemsInfo shipInfo)
        {
            Debug.Assert(data != null, "BuyButtonsEvent, data is null");
            Debug.Assert(shipInfo != null, "BuyButtonsEvent, shipInfo is null");

            Debug.Log(shipInfo.buyButton.name + "Clicked, ship name " + shipInfo.nameID);
            if (shipInfo.nameID != currentShopInfo.nameID) // if the ship is not current ship
            {
                if (!data.OwnedItems.Contains(shipInfo.nameID)) // if the ship is unowned
                {
                    if (data.Money >= shipInfo.price) // check the price
                    {
                        data.Money -= shipInfo.price;
                        UpdateMoneyDislay(data.Money.ToString());
                        data.OwnedItems.Add(shipInfo.nameID); // add new ship into the data.
                        ChangeDisplayImage(shipInfo, true);
                        SetCurrentShip(data, shipInfo.nameID); // change the current ship in the data into this
                        UpdateStatusDisplay(shipInfo, "Current"); // update the button of cliecked ship.
                        UpdateStatusDisplay(currentShopInfo, "Set"); // update the button of previous current ship.
                        currentShopInfo = shipInfo; // change saved current ship into this ship.
                    }
                    else
                    {
                        notEnoughMoneyDisplay.SetActive(true);
                        // Debug.Log("Not enough money to buy " + shipInfo.nameID);
                    }
                }
                else // click owned ship
                {
                    SetCurrentShip(data, shipInfo.nameID);
                    UpdateStatusDisplay(shipInfo, "Current"); // update the button of cliecked ship.
                    UpdateStatusDisplay(currentShopInfo, "Set"); // update the button of previous current ship.
                    currentShopInfo = shipInfo;  // change current ship into this ship.
                }
            }
            else
            {
                // Clicked using ship, do nothing...
            }
        }
        #endregion

        #region Shop setup utilities
        private bool CheckOwnedShip(NameIDs shipName, List<NameIDs> nameList)
        {
            Debug.Assert(shipName != NameIDs.None, "CheckOwnedShip, shipName is null.");
            Debug.Assert(nameList != null, "CheckOwnedShip, nameList is null.");

            if (nameList.Contains(shipName))
                return true;
            return false;
        }

        private void SetCurrentShip(PlayerData data, NameIDs shipName)
        {
            data.CurrentShip = shipName;
        }

        private void UpdateMoneyDislay(string newValue)
        {
            moneyDislay.text = newValue;
        }

        private void ChangeDisplayImage(NormalShopItemsInfo shipInfo, bool isOwned)
        {
            shipInfo.displayImage.sprite = isOwned ? shipInfo.ownedSprite : shipInfo.unownedSprite;
            shipInfo.displayImage.SetNativeSize();
        }

        private void UpdateStatusDisplay(NormalShopItemsInfo shipInfo, string message)
        {
            shipInfo.priceDisplay.text = message;
        }
        #endregion
    }
}