using System;
using System.Collections;
using UnityEngine;
using Extension.ExtraTypes;
using BaseSystems.Observer;

namespace MainGame
{
    public class Spawner : MonoBehaviour
    {
        #region Debug inspector configuration
        [Serializable]
        public class WaveConfig
        {
            public int enemyCount;
            public float timeBetweenSpawns;
        }

        [Serializable]
        public class WaveConfigList : ReorderableArray<WaveConfig> { }
        #endregion

        [SerializeField, Reorderable]
        private WaveConfigList waves;

        [SerializeField]
        private Enemy enemy;

        [SerializeField]
        private MapGenerator mapGenerator;

        [SerializeField, Range(0.01f, 5f)]
        private float spawnDelay = 1f, tileFlashSpeed = 4f;

        private WaveConfig currentWave;
        private int currentWaveNumber = 0;

        private int enemiesRemainingToSpawn = 0;
        private int enemiesRemainingAlive = 0;
        private float nextSpawnTime = 1f;

        protected virtual void Start()
        {
            NextWave();
        }

        protected virtual void Update()
        {
            if (enemiesRemainingToSpawn > 0 && Time.time > nextSpawnTime)
            {
                enemiesRemainingToSpawn--;
                nextSpawnTime = Time.time + currentWave.timeBetweenSpawns;
                StartCoroutine(SpawnEnemy());
            }
        }

        private IEnumerator SpawnEnemy()
        {
            Transform randomTile = mapGenerator.GetRandomOpenTile();
            Material tileMat = randomTile.GetComponent<Renderer>().material;
            Color initialColour = tileMat.color;
            Color flashColour = Color.red;
            float spawnTimer = 0;
            while(spawnTimer < spawnDelay)
            {
                tileMat.color = Color.Lerp(initialColour, flashColour, Mathf.PingPong(spawnTimer * tileFlashSpeed, 1));
                spawnTimer += Time.deltaTime;
                yield return null;
            }

            Enemy spawnedEnemy = Instantiate(enemy, randomTile.position + Vector3.up, Quaternion.identity) as Enemy;
            spawnedEnemy.OnDeath += OnEnemyDeath;

        }

        private void OnEnemyDeath()
        {
            enemiesRemainingAlive--;

            if (enemiesRemainingAlive == 0)
            {
                NextWave();
            }
        }

        private void NextWave()
        {
            currentWaveNumber++;
            Debug.Log("Start Wave: " + currentWaveNumber);

            if (currentWaveNumber - 1 < waves.Length)
            {
                currentWave = waves[currentWaveNumber - 1];

                enemiesRemainingToSpawn = currentWave.enemyCount;
                enemiesRemainingAlive = enemiesRemainingToSpawn;
            }
        }
    }
}
