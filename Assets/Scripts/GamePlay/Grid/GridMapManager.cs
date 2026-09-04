using UnityEngine;

public class GridMapManager : MonoBehaviour
{
    public static GridMapManager Instance { get; private set; }

    [Header("Grid")]
    [SerializeField] private Vector2Int gridSize = new(100, 100);
    [SerializeField] private float cellSize = 1f;

    [Header("Map")]
    [SerializeField] private Vector2 mapCenter;

    private GridNode[,] nodes;

    public Vector2Int GridSize => gridSize;
    public float CellSize => cellSize;
    public Vector2 MapCenter => mapCenter;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        GenerateGrid();
    }

    private void GenerateGrid()
    {
        nodes =
            new GridNode[
                gridSize.x,
                gridSize.y
            ];

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                nodes[x, y] =
                    new GridNode(
                        new Vector2Int(x, y),
                        true
                    );
            }
        }
    }

    public GridNode GetNode(Vector2Int position)
    {
        if (nodes == null)
            return null;

        if (!IsValidPosition(position))
            return null;

        return nodes[
            position.x,
            position.y
        ];
    }

    public bool IsValidPosition(
        Vector2Int position)
    {
        return position.x >= 0 &&
               position.y >= 0 &&
               position.x < gridSize.x &&
               position.y < gridSize.y;
    }

    public Vector2Int WorldToGrid(
        Vector2 worldPosition)
    {
        Vector2 bottomLeft =
            mapCenter -
            new Vector2(
                gridSize.x * cellSize,
                gridSize.y * cellSize
            ) * 0.5f;

        Vector2 localPosition =
            worldPosition -
            bottomLeft;

        return new Vector2Int(
            Mathf.FloorToInt(
                localPosition.x / cellSize
            ),
            Mathf.FloorToInt(
                localPosition.y / cellSize
            )
        );
    }

    public Vector2 GridToWorld(
        Vector2Int gridPosition)
    {
        Vector2 bottomLeft =
            mapCenter -
            new Vector2(
                gridSize.x * cellSize,
                gridSize.y * cellSize
            ) * 0.5f;

        return bottomLeft +
               new Vector2(
                   (gridPosition.x + 0.5f) *
                   cellSize,

                   (gridPosition.y + 0.5f) *
                   cellSize
               );
    }

    public void SetWalkable(
        Vector2Int position,
        bool walkable)
    {
        GridNode node =
            GetNode(position);

        if (node != null)
        {
            node.Walkable =
                walkable;
        }
    }

    public void SetAreaWalkable(
        Vector2 center,
        Vector2 size,
        bool walkable)
    {
        Vector2Int min =
            WorldToGrid(
                center - size * 0.5f
            );

        Vector2Int max =
            WorldToGrid(
                center + size * 0.5f
            );

        for (int x = min.x; x <= max.x; x++)
        {
            for (int y = min.y; y <= max.y; y++)
            {
                SetWalkable(
                    new Vector2Int(x, y),
                    walkable
                );
            }
        }
    }

    // =========================================================
    // Building Õ¼ÓÃ
    // =========================================================

    public bool Occupy(
        Vector2Int position,
        Building building)
    {
        GridNode node =
            GetNode(position);

        if (node == null)
            return false;

        if (node.IsOccupied &&
            node.Occupant != building)
        {
            return false;
        }

        node.SetOccupant(building);

        return true;
    }

    public void Release(
        Vector2Int position,
        Building building)
    {
        GridNode node =
            GetNode(position);

        if (node == null)
            return;

        node.ClearOccupant(building);
    }

    public bool IsOccupied(
        Vector2Int position)
    {
        GridNode node =
            GetNode(position);

        return node != null &&
               node.IsOccupied;
    }

    public Building GetOccupant(
        Vector2Int position)
    {
        GridNode node =
            GetNode(position);

        return node != null
            ? node.Occupant
            : null;
    }

    public bool IsAreaOccupied(
        Vector2 center,
        Vector2 size)
    {
        Vector2Int min =
            WorldToGrid(
                center - size * 0.5f
            );

        Vector2Int max =
            WorldToGrid(
                center + size * 0.5f
            );

        for (int x = min.x; x <= max.x; x++)
        {
            for (int y = min.y; y <= max.y; y++)
            {
                GridNode node =
                    GetNode(
                        new Vector2Int(x, y)
                    );

                if (node != null &&
                    node.IsOccupied)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public Vector2 GetMapSize()
    {
        return new Vector2(
            gridSize.x * cellSize,
            gridSize.y * cellSize
        );
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 mapSize =
            GetMapSize();

        Gizmos.DrawWireCube(
            mapCenter,
            mapSize
        );
    }
}