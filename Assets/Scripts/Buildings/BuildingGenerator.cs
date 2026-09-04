using System;
using UnityEngine;


[Serializable]
public class BuildingEntry
{
    public BuildingData data;
    public Building prefab;
}


public class BuildingGenerator : MonoBehaviour
{
    public static BuildingGenerator Instance { get; private set; }

    [SerializeField]
    private BuildingEntry[] buildings;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }


    public Building Generate(
        BuildingData data,
        Vector2Int gridPosition)
    {
        if (!CanGenerate(data, gridPosition))
            return null;

        Building prefab = FindPrefab(data);

        if (prefab == null)
        {
            Debug.LogError(
                $"找不到建筑预制体：{data.Name}"
            );

            return null;
        }

        GridMapManager map =
            GridMapManager.Instance;

        Building building = Instantiate(
            prefab,
            map.GridToWorld(gridPosition),
            Quaternion.identity
        );

        building.Initialize(data);

        building.Build(gridPosition);

        return building;
    }


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

        if (cells == null || cells.Length == 0)
            return false;

        for (int i = 0; i < cells.Length; i++)
        {
            Vector2Int position =
                gridPosition + cells[i];

            GridNode node =
                map.GetNode(position);

            // 超出地图范围
            if (node == null)
                return false;

            // 地块不可建造
            if (!node.Walkable)
                return false;

            // 已经被占用
            if (node.IsOccupied)
                return false;
        }

        return true;
    }


    private Building FindPrefab(
        BuildingData data)
    {
        if (buildings == null)
            return null;

        for (int i = 0; i < buildings.Length; i++)
        {
            if (buildings[i].data == data)
                return buildings[i].prefab;
        }

        return null;
    }
}