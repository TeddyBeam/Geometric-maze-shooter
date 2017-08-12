using System.Collections;
using UnityEngine;
using Extension.ExtraTypes;
using Extension.Attributes;
using BaseSystems.Observer;
using MainGame.Maps;

namespace MainGame
{
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField, Reorderable]
        private EnemtSpawnWaveList waves;

        [SerializeField]
        private MapGenerator mapGenerator;

        [SerializeField, Range(0.01f, 5f)]
        private float spawnDelay = 1f, tileFlashSpeed = 4f;

        private EnemtSpawnWave currentWave;
        private System.Action<object> OnPlayerCampingDetectedHandler;
        private int currentWaveNumber = 0;
        private int enemiesRemainingToSpawn = 0;
        private int enemiesRemainingAlive = 0;
        private bool isQuitting = false;

        protected virtual void Start()
        {
            // Listen to OnPlayerCampingDetected from CampingPunisher.
            OnPlayerCampingDetectedHandler = (playerPosition) => OnPlayerCampingDetected((Vector3)playerPosition);
            this.RegisterListener(ObserverEventID.OnPlayerCampingDetected, OnPlayerCampingDetectedHandler);

            NextWave();
            StartCoroutine(NormalSpawn());
        }

        protected virtual void OnApplicationQuit()
        {
            isQuitting = true;
        }

        protected virtual void OnDestroy()
        {
            if(!isQuitting)
                this.RemoveListener(ObserverEventID.OnPlayerCampingDetected, OnPlayerCampingDetectedHandler);
        }

        private IEnumerator NormalSpawn()
        {
            // Wait sometime when the game has just been started.
            yield return new WaitForSeconds(currentWave.timeBetweenSpawns);

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
            enemiesRemainingToSpawn--;
            Enemy spawnedEnemy = Instantiate(currentWave.enemies[Random.Range(0, currentWave.enemies.Length)], spawnTile.position + Vector3.up, Quaternion.identity) as Enemy;
            spawnedEnemy.OnDeath += OnEnemyDeath;
            yield break;
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
