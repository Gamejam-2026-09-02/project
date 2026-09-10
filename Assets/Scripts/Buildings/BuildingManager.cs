using System;
using System.Collections.Generic;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    public static BuildingManager Instance { get; private set; }

    private readonly List<Building> buildings = new();

    // 新增：暴露只读视图，供 BuildingConnectionManager 等外部直接复用，
    // 避免重复用 FindObjectsByType 做全场景扫描
    public IReadOnlyList<Building> Buildings => buildings;

    public event Action OnBuildingChanged;

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

    public void Register(Building building, bool notify = true)
    {
        if (building == null)
            return;

        if (!buildings.Contains(building))
        {
            buildings.Add(building);

            if (notify)
            {
                NotifyChanged();
            }
        }
    }

    public void Unregister(Building building)
    {
        if (building == null)
            return;

        if (buildings.Remove(building))
        {
            NotifyChanged();
        }
    }

    public void NotifyChangedManually()
    {
        NotifyChanged();
    }

    private void NotifyChanged()
    {
        OnBuildingChanged?.Invoke();
    }

    public Building FindNearest(
        Vector2 position,
        BuildingType type)
    {
        Building result = null;
        float minDistance =
            float.MaxValue;

        for (int i = 0; i < buildings.Count; i++)
        {
            Building building =
                buildings[i];

            if (building == null)
                continue;

            if (building.Type != type)
                continue;

            float distance =
                ((Vector2)building.transform.position -
                 position)
                .sqrMagnitude;

            if (distance < minDistance)
            {
                minDistance = distance;
                result = building;
            }
        }

        return result;
    }
}