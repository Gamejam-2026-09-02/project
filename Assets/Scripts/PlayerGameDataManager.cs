using System.Collections.Generic;
using UnityEngine;

public class PlayerGameDataManager : MonoBehaviour
{
    public static PlayerGameDataManager Instance { get; private set; }


    [Header("初始资源")]
    [SerializeField]
    private ResourceCost[] initialResources;


    [Header("主城")]
    [SerializeField]
    private int initialMainCityHealth = 100;


    [Header("波次")]
    [SerializeField]
    private int initialWave = 1;



    public PlayerGameData Data { get; private set; }



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
        Data = new PlayerGameData();


        for (int i = 0; i < initialResources.Length; i++)
        {
            Data.Resources[
                initialResources[i].type
            ] =
            initialResources[i].amount;
        }


        Data.MainCityHealth =
            initialMainCityHealth;


        Data.MainCityMaxHealth =
            initialMainCityHealth;


        Data.CurrentWave =
            initialWave;
    }

    public int GetResource(ResourceType type)
    {
        if (Data == null ||
            Data.Resources == null)
        {
            return 0;
        }


        if (Data.Resources.TryGetValue(
                type,
                out int value))
        {
            return value;
        }


        return 0;
    }



    public void AddResource(
        ResourceType type,
        int amount)
    {
        if (Data.Resources == null)
        {
            Data.Resources =
                new Dictionary<ResourceType, int>();
        }


        if (!Data.Resources.ContainsKey(type))
        {
            Data.Resources[type] = 0;
        }


        Data.Resources[type] += amount;
    }
}