using System.Collections.Generic;
using UnityEngine;


public class RTSUnitManager : MonoBehaviour
{
    public static RTSUnitManager Instance { get; private set; }


    private readonly List<UnitMover> units =
        new();



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
        if (BuildingManager.Instance != null)
        {
            BuildingManager.Instance
                .OnBuildingChanged +=
                NotifyBuildingChanged;
        }
    }



    private void OnDisable()
    {
        if (BuildingManager.Instance != null)
        {
            BuildingManager.Instance
                .OnBuildingChanged -=
                NotifyBuildingChanged;
        }
    }



    private void NotifyBuildingChanged()
    {
        for (int i = 0; i < units.Count; i++)
        {
            units[i].OnBuildingChanged();
        }
    }



    public void Register(UnitMover unit)
    {
        if (!units.Contains(unit))
            units.Add(unit);
    }



    public void Unregister(UnitMover unit)
    {
        units.Remove(unit);
    }



    public List<UnitMover> GetUnits()
    {
        return units;
    }

    public UnitMover FindNearest(Vector2 position)
    {
        List<UnitMover> units = GetUnits();

        if (units == null || units.Count == 0)
            return null;


        UnitMover nearest = null;

        float minDistance = float.MaxValue;


        for (int i = 0; i < units.Count; i++)
        {
            UnitMover unit = units[i];

            if (unit == null)
                continue;


            float distance =
                ((Vector2)unit.transform.position - position)
                .sqrMagnitude;


            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = unit;
            }
        }


        return nearest;
    }
}