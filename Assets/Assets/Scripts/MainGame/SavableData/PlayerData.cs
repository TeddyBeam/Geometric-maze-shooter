using System;
using System.Collections.Generic;
using UnityEngine;

namespace MainGame.Data
{
    /// <summary>
    /// User's save data.
    /// </summary>
    [Serializable]
    public class PlayerData
    {
        [SerializeField]
        private Dictionary<GunsName, GunData> ownedGuns;

        [SerializeField]
        private GunsName usingGun;

        [SerializeField]
        private int money;

        [SerializeField]
        private int highestScore;

        public Dictionary<GunsName, GunData> OwnedGuns { get { return ownedGuns; } set { ownedGuns = value; } }
        public GunsName UsingGun { get { return usingGun; } set { usingGun = value; } }
        public int Money { get { return money; } set { money = value; } }
        public int HighestScore { get { return highestScore; } set { highestScore = value; } }

        public PlayerData(GunsName defaultGun, GunData defaultGunData, int defaultMoney = 1000, int score = 0)
        {
            ownedGuns = new Dictionary<GunsName, GunData>();
            ownedGuns.Add(defaultGun, defaultGunData);

            usingGun = defaultGun;
            money = defaultMoney;
            highestScore = score;
        }
    }
}