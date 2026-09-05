using System;
using UnityEngine;

public class GridMapManager : MonoBehaviour
{
    public static GridMapManager Instance { get; private set; }


    [Header("Grid")]
    [SerializeField]
    private Vector2Int gridSize = new(100, 100);

    [SerializeField]
    private float cellSize = 1f;


    [Header("Map")]
    [SerializeField]
    private Vector2 mapCenter;



    private GridNode[,] nodes;


    public Vector2Int GridSize => gridSize;

    public float CellSize => cellSize;

    public Vector2 MapCenter => mapCenter;



    /// <summary>
    /// 地图格子变化事件
    /// </summary>
    public event Action<Vector2Int> OnGridChanged;



    private void Awake()
    {
        if (Instance != null &&
           Instance != this)
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



    public bool IsValidPosition(Vector2Int position)
    {
        return position.x >= 0 &&
               position.y >= 0 &&
               position.x < gridSize.x &&
               position.y < gridSize.y;
    }



    public Vector2Int WorldToGrid(Vector2 worldPosition)
    {
        Vector2 bottomLeft =
            mapCenter -
            new Vector2(
                gridSize.x * cellSize,
                gridSize.y * cellSize
            ) * 0.5f;


        Vector2 local =
            worldPosition -
            bottomLeft;


        return new Vector2Int(
            Mathf.FloorToInt(local.x / cellSize),
            Mathf.FloorToInt(local.y / cellSize)
        );
    }



    public Vector2 GridToWorld(Vector2Int gridPosition)
    {
        Vector2 bottomLeft =
            mapCenter -
            new Vector2(
                gridSize.x * cellSize,
                gridSize.y * cellSize
            ) * 0.5f;


        return bottomLeft +
               new Vector2(
                   (gridPosition.x + 0.5f) * cellSize,
                   (gridPosition.y + 0.5f) * cellSize
               );
    }



    public bool IsWalkable(Vector2Int position)
    {
        GridNode node =
            GetNode(position);


        return node != null &&
               node.Walkable &&
               !node.IsOccupied;
    }



    public void SetWalkable(
        Vector2Int position,
        bool walkable)
    {
        GridNode node =
            GetNode(position);


        if (node == null)
            return;


        node.Walkable = walkable;


        NotifyChanged(position);
    }



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

        // 建筑占用格子不可通行
        node.Walkable = false;


        NotifyChanged(position);


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

        node.Walkable = true;


        NotifyChanged(position);
    }


    private void NotifyChanged(Vector2Int position)
    {
        OnGridChanged?.Invoke(position);
    }



    public bool IsOccupied(Vector2Int position)
    {
        GridNode node =
            GetNode(position);


        return node != null &&
               node.IsOccupied;
    }



    public Building GetOccupant(Vector2Int position)
    {
        GridNode node =
            GetNode(position);


        return node != null
            ? node.Occupant
            : null;
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
        Gizmos.DrawWireCube(
            mapCenter,
            GetMapSize()
        );
    }
}