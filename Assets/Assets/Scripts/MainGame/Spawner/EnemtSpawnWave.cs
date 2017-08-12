using System;
using UnityEngine;
using Extension.Attributes;
using Extension.ExtraTypes;

namespace MainGame
{
    [Serializable]
    public class EnemtSpawnWaveList : ReorderableArray<EnemtSpawnWave> { }

    [Serializable]
    public class EnemyList : ReorderableArray<Enemy> { }

    [Serializable]
    public class EnemtSpawnWave
    {
        [Positive]
        public int enemyCount;
        [Positive]
        public float timeBetweenSpawns;

        [Reorderable]
        public EnemyList enemies;
    }
}
