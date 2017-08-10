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
        private Action<object> OnPlayerCampingDetectedHandler;
        private int currentWaveNumber = 0;

        private int enemiesRemainingToSpawn = 0;
        private int enemiesRemainingAlive = 0;

        protected virtual void Start()
        {
            // Listen to OnPlayerCampingDetected from CampingPunisher.
            OnPlayerCampingDetectedHandler = (playerPosition) => OnPlayerCampingDetected((Vector3)playerPosition);
            this.RegisterListener(ObserverEventID.OnPlayerCampingDetected, OnPlayerCampingDetectedHandler);

            NextWave();
            StartCoroutine(NormalSpawn());
        }

        protected virtual void OnDestroy()
        {
            this.RemoveListener(ObserverEventID.OnPlayerCampingDetected, OnPlayerCampingDetectedHandler);
        }

        private IEnumerator NormalSpawn()
        {
            while (true)
            {
                if (enemiesRemainingToSpawn > 0)
                {
                    Transform spawnTile = mapGenerator.GetRandomOpenTile();
                    StartCoroutine(SpawnEnemy(spawnTile));

                    yield return new WaitForSeconds(currentWave.timeBetweenSpawns);
                }
                yield return new WaitForFixedUpdate();
            }
        }

        private void OnPlayerCampingDetected(Vector3 playerPosition)
        {
            Transform spawnTile = mapGenerator.GetTileFromPosition(playerPosition);
            Debug.Log("Camping extra enemy spawned.");
            StartCoroutine(SpawnEnemy(spawnTile));
        }

        private IEnumerator SpawnEnemy(Transform spawnTile)
        {
            enemiesRemainingToSpawn--;
            Material tileMat = spawnTile.GetComponent<Renderer>().material;
            Color initialColour = tileMat.color;
            Color flashColour = Color.red;
            float spawnTimer = 0;
            while (spawnTimer < spawnDelay)
            {
                tileMat.color = Color.Lerp(initialColour, flashColour, Mathf.PingPong(spawnTimer * tileFlashSpeed, 1));
                spawnTimer += Time.deltaTime;
                yield return new WaitForFixedUpdate();
            }

            Enemy spawnedEnemy = Instantiate(enemy, spawnTile.position + Vector3.up, Quaternion.identity) as Enemy;
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
