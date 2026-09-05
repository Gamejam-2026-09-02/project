using System;
using System.Collections.Generic;
using UnityEngine;


public class BuildingUnlockManager : MonoBehaviour
{
    public static BuildingUnlockManager Instance { get; private set; }



    [Header("初始解锁建筑")]
    [SerializeField]
    private BuildingData[] initialBuildings;



    private List<BuildingData> unlockedBuildings =
        new();



    public IReadOnlyList<BuildingData> UnlockedBuildings =>
        unlockedBuildings;



    public event Action OnBuildingListChanged;



    private void Awake()
    {
        if (Instance != null &&
           Instance != this)
        {
            Destroy(gameObject);
            return;
        }


        Instance = this;


        Initialize();
    }



    private void Initialize()
    {
        unlockedBuildings.Clear();


        for (int i = 0; i < initialBuildings.Length; i++)
        {
            if (initialBuildings[i] != null)
            {
                unlockedBuildings.Add(
                    initialBuildings[i]
                );
            }
        }


        OnBuildingListChanged?.Invoke();
    }



    public bool UnlockBuilding(
        BuildingData data)
    {
        if (data == null)
            return false;


        if (unlockedBuildings.Contains(data))
            return false;


        unlockedBuildings.Add(data);


        Debug.Log(
            $"解锁建筑: {data.name}"
        );


        OnBuildingListChanged?.Invoke();


        return true;
    }
}