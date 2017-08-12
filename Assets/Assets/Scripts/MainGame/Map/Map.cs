using System;
using UnityEngine;
using Extension.Attributes;
using Extension.ExtraTypes;

namespace MainGame.Maps
{
    [Serializable]
    public class MapList : ReorderableArray<Map> { }

    [Serializable]
    public class TransformList : ReorderableArray<Transform> { }

    [Serializable]
    public class Map
    {
        public IntVector2 mapSize = new IntVector2(20, 20);

        public Color foregroundColor = Color.white, backgroundColor = Color.black;

        [Range(0, 1)]
        public float obstaclePercent = 0.1f, outlinePercent = 0.1f;

        [Positive]
        public float tileSize = 1;

        public int shuffeSeed = 0;

        public Vector2 obstacleHeightRange = Vector2.one;

        public bool setObstacleColor = false;

        [Reorderable]
        public TransformList obstaclePrefabs;

        public IntVector2 mapCentre
        {
            get
            {
                return new IntVector2(mapSize.x / 2, mapSize.y / 2);
            }
        }

        public Map(IntVector2 mapSize, Vector2 obstacleHeightRange, Color foregroundColor, Color backgroundColor, float obstaclePercent,
            float outlinePercent = 0, float tileSize = 1f, int shuffeSeed = 1)
        {
            this.mapSize = mapSize;
            this.obstacleHeightRange = obstacleHeightRange;
            this.foregroundColor = foregroundColor;
            this.backgroundColor = backgroundColor;
            this.obstaclePercent = obstaclePercent;
            this.outlinePercent = outlinePercent;
            this.shuffeSeed = shuffeSeed;
        }
    }
}
