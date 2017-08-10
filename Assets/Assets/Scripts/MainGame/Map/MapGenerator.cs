using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Extension.ExtraTypes;
using Extension.Attributes;
using BaseSystems.Utilities;

namespace MainGame
{
    public class MapGenerator : MonoBehaviour
    {
        #region Init
        [SerializeField]
        private Transform tilePrefab, obstaclePrefab;

        [SerializeField]
        private BoxCollider groundCollider;

        [SerializeField]
        private NavMeshSurface navigationGround;

        [SerializeField, Positive]
        private float tileSize = 1f;

        [SerializeField]
        private bool setObstacleColor = false;

        [SerializeField, Reorderable, Space(10)]
        private MapList maps;

        [SerializeField, NonNegative]
        private int currentMapIndex = 0;

        private List<IntVector2> allTilesCoord;
        private Queue<IntVector2> shuffedTilesCoord;
        private Queue<IntVector2> openTilesCoord;
        private Transform[,] tilesMap;
        private Map currentMap;
        #endregion

        #region Mono behaviours
        protected virtual void Awake()
        {
            GenerateMap();
        }

        public void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(navigationGround.transform.localScale.x, 5f, navigationGround.transform.localScale.y));
        }
        #endregion

        #region Generate map
        [InspectorButton]
        public void GenerateMap()
        {
            /// Set the current map index
            if (currentMapIndex < maps.Length)
            {
                currentMap = maps[currentMapIndex];
                GenerateMap(currentMap.mapSize);
            }
            else
            {
                Debug.LogError("currentMapIndex is out of range.");
                return;
            }
        }

        public void GenerateMap(IntVector2 mapSize)
        {
            #region Debug (Reactive generate)
#if UNITY_EDITOR
            string holderName = "Generated Map";
            if (transform.Find(holderName))
            {
                DestroyImmediate(transform.Find(holderName).gameObject);
            }

            Transform mapHolder = new GameObject(holderName).transform;
            mapHolder.parent = transform;
#endif
            #endregion

            /// Scale the ground collider size
            groundCollider.size = new Vector3(currentMap.mapSize.x * tileSize, 0.5f, currentMap.mapSize.y * tileSize);
            
            /// Generate tiles's coordinate.
            allTilesCoord = new List<IntVector2>();
            for (int i = 0; i < mapSize.x; i++)
            {
                for (int j = 0; j < mapSize.y; j++)
                {
                    allTilesCoord.Add(new IntVector2(i, j));
                }
            }

            /// Then shuffed it.
            shuffedTilesCoord = new Queue<IntVector2>(new MathUtilities().ShuffeList(allTilesCoord, currentMap.shuffeSeed));

            /// Geenerate the ground tiles
            tilesMap = new Transform[currentMap.mapSize.x, currentMap.mapSize.y];
            for (int i = 0; i < mapSize.x; i++)
            {
                for (int j = 0; j < mapSize.y; j++)
                {
                    Vector3 tilePosition = CoordToMapPosition(mapSize, i, j);
                    Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90f));
                    tilesMap[i, j] = newTile; // Save the tile's transform for later use.
                    newTile.localScale = Vector3.one * (1 - currentMap.outlinePercent) * tileSize;

                    #region Debug (Reactive generate)
#if UNITY_EDITOR
                    newTile.parent = mapHolder;
#endif
                    #endregion
                }
            }

            /// Generate obstacle
            bool[,] obstacleMap = new bool[mapSize.x, mapSize.y];

            int obstacleCount = (int)(mapSize.x * mapSize.y * currentMap.obstaclePercent);
            List<IntVector2> allOpenCoords = new List<IntVector2>(allTilesCoord);
                
            for (int i = 0, currentObstacleCount = 0; i < obstacleCount; i++)
            {
                IntVector2 randomCoord = GetRandomCoord();
                    
                obstacleMap[randomCoord.x, randomCoord.y] = true;
                currentObstacleCount++;

                if (randomCoord != currentMap.mapCentre  && IsMapFullyAccessible(obstacleMap, currentObstacleCount))
                {
                    float randomHeight = Random.Range(currentMap.obstacleHeightRange.x, currentMap.obstacleHeightRange.y);
                    Vector3 obstacleSize = new Vector3((1 - currentMap.outlinePercent) * tileSize, randomHeight, (1 - currentMap.outlinePercent) * tileSize);
                    // Random a position and make sure the obstacle is always on the ground.
                    Vector3 obstaclePosition = CoordToMapPosition(mapSize, randomCoord.x, randomCoord.y) + Vector3.up * obstacleSize.y * 0.5f;
                    Transform newObstacle = Instantiate(obstaclePrefab, obstaclePosition, Quaternion.identity);
                    newObstacle.localScale = obstacleSize;
                    allOpenCoords.Remove(randomCoord);

                    /// Set obstacle's color
                    if (setObstacleColor)
                    {
                        Renderer obstacleRenderer = newObstacle.GetComponent<Renderer>();
                        Material obstacleMaterial = new Material(obstacleRenderer.sharedMaterial);
                        float colourPercent = randomCoord.y / (float)currentMap.mapSize.y;
                        obstacleMaterial.color = Color.Lerp(currentMap.foregroundColor, currentMap.backgroundColor, colourPercent);
                        obstacleRenderer.sharedMaterial = obstacleMaterial;
                    }
                    #region Debug (Reactive generate)
#if UNITY_EDITOR
                    newObstacle.parent = mapHolder;
#endif
                    #endregion
                }
                else
                {
                    obstacleMap[randomCoord.x, randomCoord.y] = false;
                    currentObstacleCount--;
                }
            }

            openTilesCoord = new Queue<IntVector2>(new MathUtilities().ShuffeList(allTilesCoord, currentMap.shuffeSeed));

            /// Scale the navigation surface size and bake the navigation.
            navigationGround.transform.localScale = new Vector3(mapSize.x, mapSize.y) * tileSize;
            navigationGround.BuildNavMesh();

            Debug.Log("Map generated successfully.");
        }
        #endregion

        #region Map generate utilities
        public Transform GetRandomOpenTile()
        {
            if (openTilesCoord != null)
            {
                IntVector2 randomCoord = openTilesCoord.Dequeue();
                openTilesCoord.Enqueue(randomCoord);
                return tilesMap[randomCoord.x, randomCoord.y];
            }
            else
            {
                Debug.LogError("Null TilesCoord");
                return null;
            }
        }

        public Transform GetTileFromPosition(Vector3 position)
        {
            int x = Mathf.RoundToInt(position.x / tileSize + (currentMap.mapSize.x - 1) / 2f);
            int y = Mathf.RoundToInt(position.z / tileSize + (currentMap.mapSize.x - 1) / 2f);

            /// Make sure x & y are contained within the tiles map.
            x = Mathf.Clamp(x, 0, tilesMap.GetLength(0) - 1);
            y = Mathf.Clamp(y, 0, tilesMap.GetLength(1) - 1);

            return tilesMap[x, y];
        }

        private IntVector2 GetRandomCoord()
        {
            IntVector2 randomCoord = shuffedTilesCoord.Dequeue();
            shuffedTilesCoord.Enqueue(randomCoord);
            return randomCoord;
        }

        private Vector3 CoordToMapPosition(IntVector2 mapSize, int xCoordinate, int yCoordinate)
        {
            return new Vector3(-mapSize.x / 2f + 0.5f + xCoordinate, 0f, -mapSize.y / 2f + 0.5f + yCoordinate) * tileSize;
        }

        private bool IsMapFullyAccessible(bool[,] obstacleMap, int currentObstacleCount)
        {
            bool[,] mapFlags = new bool[obstacleMap.GetLength(0), obstacleMap.GetLength(1)];
            Queue<IntVector2> queue = new Queue<IntVector2>();
            queue.Enqueue(currentMap.mapCentre);
            mapFlags[currentMap.mapCentre.x, currentMap.mapCentre.y] = true;

            int accessibleTileCount = 1;

            while (queue.Count > 0)
            {
                IntVector2 tile = queue.Dequeue();

                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        int neighbourX = tile.x + x;
                        int neighbourY = tile.y + y;
                        if (x == 0 || y == 0)
                        {
                            if (neighbourX >= 0 && neighbourX < obstacleMap.GetLength(0) && neighbourY >= 0 && neighbourY < obstacleMap.GetLength(1))
                            {
                                if (!mapFlags[neighbourX, neighbourY] && !obstacleMap[neighbourX, neighbourY])
                                {
                                    mapFlags[neighbourX, neighbourY] = true;
                                    queue.Enqueue(new IntVector2(neighbourX, neighbourY));
                                    accessibleTileCount++;
                                }
                            }
                        }
                    }
                }
            }

            int targetAccessibleTileCount = currentMap.mapSize.x * currentMap.mapSize.y - currentObstacleCount;
            return targetAccessibleTileCount == accessibleTileCount;
        }
        #endregion
    }
}