using System;
using System.Collections.Generic;
using UnityEngine;

namespace BaseSystems.Data.Storage
{
    /// <summary>
    /// User's save data.
    /// </summary>
    [Serializable]
    public class PlayerData
    {
        [SerializeField]
        private List<NameIDs> ownedItem;

        [SerializeField]
        private NameIDs currentItem;

        [SerializeField]
        private int money;

        [SerializeField]
        private int highestScore;

        public List<NameIDs> OwnedItems { get { return ownedItem; } set { ownedItem = value; } }
        public NameIDs CurrentShip { get { return currentItem; } set { currentItem = value; } }
        public int Money { get { return money; } set { money = value; } }
        public int HighestScore { get { return highestScore; } set { highestScore = value; } }

        public PlayerData(NameIDs defaultItem = NameIDs.Ship01, int defaultMoney = 1000, int score = 0)
        {
            ownedItem = new List<NameIDs>() { defaultItem };
            currentItem = defaultItem;
            money = defaultMoney;
            highestScore = score;
        }
    }
}