using System;
using UnityEngine;

[Serializable]
public class BuildingEntry
{
    public BuildingData data;

    public Building prefab;

    [Tooltip("这个建筑是否属于 Root。")]
    public bool isRoot;
}

public class BuildingGenerator : MonoBehaviour
{
    public static BuildingGenerator Instance { get; private set; }

    [SerializeField]
    private BuildingEntry[] buildings;

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // =========================================================
    // 生成
    // =========================================================

    public Building Generate(
        BuildingData data,
        Vector2Int gridPosition)
    {
        if (!CanGenerate(
                data,
                gridPosition))
        {
            return null;
        }

        BuildingEntry entry =
            FindEntry(data);

        if (entry == null ||
            entry.prefab == null)
        {
            Debug.LogError(
                $"找不到建筑预制体：" +
                $"{data.Name}"
            );

            return null;
        }

        GridMapManager map =
            GridMapManager.Instance;

        Building building =
            Instantiate(
                entry.prefab,
                map.GridToWorld(
                    gridPosition
                ),
                Quaternion.identity
            );

        building.Initialize(
            entry.isRoot
        );

        building.Build(
            gridPosition
        );

        return building;
    }

    // =========================================================
    // 能否生成
    // =========================================================

    public bool CanGenerate(
        BuildingData data,
        Vector2Int gridPosition)
    {
        if (data == null)
            return false;

        GridMapManager map =
            GridMapManager.Instance;

        if (map == null)
            return false;

        Vector2Int[] cells =
            data.GetOccupiedCells();

        if (cells == null ||
            cells.Length == 0)
        {
            return false;
        }

        for (int i = 0; i < cells.Length; i++)
        {
            Vector2Int position =
                gridPosition + cells[i];

            GridNode node =
                map.GetNode(position);

            if (node == null)
                return false;

            if (!node.Walkable)
                return false;

            if (node.IsOccupied)
                return false;
        }

        return true;
    }

    // =========================================================
    // 获取预制体
    // =========================================================

    public Building GetPrefab(
        BuildingData data)
    {
        BuildingEntry entry =
            FindEntry(data);

        return entry != null
            ? entry.prefab
            : null;
    }

    // =========================================================
    // 获取 Entry
    // =========================================================

    public bool IsRoot(
        BuildingData data)
    {
        BuildingEntry entry =
            FindEntry(data);

        return entry != null &&
               entry.isRoot;
    }

    private BuildingEntry FindEntry(
        BuildingData data)
    {
        if (data == null ||
            buildings == null)
        {
            return null;
        }

        for (int i = 0; i < buildings.Length; i++)
        {
            BuildingEntry entry =
                buildings[i];

            if (entry == null)
                continue;

            if (entry.data == data)
            {
                return entry;
            }
        }

        return null;
    }
}