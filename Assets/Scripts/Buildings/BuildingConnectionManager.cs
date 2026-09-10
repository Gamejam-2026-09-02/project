using System.Collections.Generic;
using UnityEngine;

public class BuildingConnectionManager : MonoBehaviour
{
    public static BuildingConnectionManager Instance { get; private set; }

    private Building home;

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

    private void OnEnable()
    {
        BuildingEvents.OnBuilt += OnBuildingChanged;
        BuildingEvents.OnDestroyed += OnBuildingChanged;
    }

    private void OnDisable()
    {
        BuildingEvents.OnBuilt -= OnBuildingChanged;
        BuildingEvents.OnDestroyed -= OnBuildingChanged;
    }

    private void OnBuildingChanged(
        Building building)
    {
        Refresh();
    }

    /// <summary>
    /// 重新计算所有建筑与主城的连接状态
    /// </summary>
    public void Refresh()
    {
        if (home == null ||
            GridMapManager.Instance == null ||
            BuildingManager.Instance == null)
        {
            return;
        }

        HashSet<Building> connected =
            new HashSet<Building>();

        Queue<Building> queue =
            new Queue<Building>();

        connected.Add(home);
        queue.Enqueue(home);

        while (queue.Count > 0)
        {
            Building current =
                queue.Dequeue();

            foreach (Building next in GetNeighbours(current))
            {
                if (next == null)
                    continue;

                if (connected.Add(next))
                {
                    queue.Enqueue(next);
                }
            }
        }

        // 修改：改用 BuildingManager 已维护的列表，去掉 FindObjectsByType 全场景扫描
        IReadOnlyList<Building> buildings =
            BuildingManager.Instance.Buildings;

        for (int i = 0; i < buildings.Count; i++)
        {
            Building building =
                buildings[i];

            if (building == null)
                continue;

            building.SetConnection(
                connected.Contains(building)
            );
        }
    }

    /// <summary>
    /// 获取相邻建筑
    /// </summary>
    private List<Building> GetNeighbours(
     Building building)
    {
        List<Building> result =
            new();

        if (building == null ||
            building.OccupiedCells == null)
        {
            return result;
        }

        HashSet<Building> checkedBuildings =
            new();

        foreach (Vector2Int offset in building.OccupiedCells)
        {
            Vector2Int worldCell =
                building.GridPosition + offset;

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0)
                        continue;

                    AddNeighbour(
                        worldCell + new Vector2Int(x, y),
                        checkedBuildings,
                        result
                    );
                }
            }
        }

        return result;
    }

    private void AddNeighbour(
        Vector2Int position,
        HashSet<Building> checkedBuildings,
        List<Building> result)
    {
        Building other =
            GridMapManager.Instance
            .GetOccupant(position);

        if (other == null)
            return;

        if (checkedBuildings.Add(other))
        {
            result.Add(other);
        }
    }

    public void SetHome(Building building)
    {
        home = building;
    }
}