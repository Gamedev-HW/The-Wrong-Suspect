using System.Collections.Generic;
using UnityEngine;
using WrongSuspect.Events;

namespace WrongSuspect.Navigation
{
    /// <summary>
    /// The walkable floor of a room as a grid of small cells. It marks every cell covered by a collider on the
    /// obstacle layers (walls, furniture footprints, closed doors) as blocked, and rebuilds when a door changes.
    /// Sits on the room object and covers the room sprite.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class NavGrid : MonoBehaviour
    {
        [SerializeField, Tooltip("World units per cell.")]
        private float cellSize = 0.25f;
        [SerializeField, Tooltip("Gap kept between the culprit's feet and walls or furniture.")]
        private float clearance = 0.12f;
        [SerializeField] private LayerMask obstacleLayers;

        [Header("Listens to")]
        [SerializeField, Tooltip("Raised when a door opens or closes: the grid is rebuilt.")]
        private GameEvent obstaclesChanged;

        private bool[,] walkable;
        private Vector2 origin;

        public int Width { get; private set; }
        public int Height { get; private set; }

        private void OnEnable()
        {
            if (obstaclesChanged != null) obstaclesChanged.Raised += Build;
        }

        private void OnDisable()
        {
            if (obstaclesChanged != null) obstaclesChanged.Raised -= Build;
        }

        private void Start()
        {
            Build();
        }

        /// <summary>Reads the obstacles again (automatically when a door opens or closes).</summary>
        public void Build()
        {
            Physics2D.SyncTransforms();
            Bounds area = GetComponent<SpriteRenderer>().bounds;
            origin = area.min;
            Width = Mathf.CeilToInt(area.size.x / cellSize);
            Height = Mathf.CeilToInt(area.size.y / cellSize);
            walkable = new bool[Width, Height];

            var filter = new ContactFilter2D { useTriggers = false };
            filter.SetLayerMask(obstacleLayers);
            var hits = new List<Collider2D>();
            Vector2 probe = Vector2.one * (cellSize + clearance * 2f);
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    walkable[x, y] = Physics2D.OverlapBox(CellCenter(new Vector2Int(x, y)), probe, 0f, filter, hits) == 0;
                }
            }
        }

        /// <summary>Fills <paramref name="path"/> with waypoints to <paramref name="to"/>, or to the closest reachable point.</summary>
        public PathResult FindPath(Vector2 from, Vector2 to, List<Vector2> path)
        {
            if (walkable == null)
            {
                Build();
            }
            return GridPathfinder.FindPath(this, from, to, path);
        }

        public bool IsWalkable(Vector2Int cell) =>
            walkable != null && cell.x >= 0 && cell.y >= 0 && cell.x < Width && cell.y < Height && walkable[cell.x, cell.y];

        public Vector2Int CellAt(Vector2 world) =>
            new Vector2Int(Mathf.FloorToInt((world.x - origin.x) / cellSize), Mathf.FloorToInt((world.y - origin.y) / cellSize));

        public Vector2 CellCenter(Vector2Int cell) =>
            origin + new Vector2((cell.x + 0.5f) * cellSize, (cell.y + 0.5f) * cellSize);

        // Select the room in Play mode to see the blocked cells in the Scene view.
        private void OnDrawGizmosSelected()
        {
            if (walkable == null)
            {
                return;
            }
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.35f);
            Vector3 size = new Vector3(cellSize, cellSize, 0.01f);
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (!walkable[x, y])
                    {
                        Gizmos.DrawCube(CellCenter(new Vector2Int(x, y)), size);
                    }
                }
            }
        }
    }
}
