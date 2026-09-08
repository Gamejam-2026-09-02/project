using System;
using System.Collections.Generic;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    public static BuildingManager Instance { get; private set; }


    private readonly List<Building> buildings = new();


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



    // 修改：新增 notify 参数，默认 true 不影响现有调用方
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

    // 新增：供批量生成流程在整批完成后手动触发一次通知
    public void NotifyChangedManually()
    {
        NotifyChanged();
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