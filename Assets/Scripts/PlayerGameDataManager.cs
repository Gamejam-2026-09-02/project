using UnityEngine;

public class PlayerGameDataManager : MonoBehaviour
{
    public static PlayerGameDataManager Instance { get; private set; }

    [Header("初始数据")]
    [SerializeField] private int initialWater = 100;
    [SerializeField] private int initialWood = 0;
    [SerializeField] private int initialStone = 0;

    [SerializeField] private int initialMainCityHealth = 100;

    [SerializeField] private int initialWave = 1;

    public PlayerGameData Data { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        Initialize();
    }

    private void Initialize()
    {
        Data = new PlayerGameData
        {
            Water = initialWater,
            Wood = initialWood,
            Stone = initialStone,

            MainCityHealth = initialMainCityHealth,
            MainCityMaxHealth = initialMainCityHealth,

            CurrentWave = initialWave
        };
    }

    // -------------------------
    // 资源
    // -------------------------

    public int GetResource(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Gold:
                return Data.Water;

            case ResourceType.Wood:
                return Data.Wood;

            case ResourceType.Stone:
                return Data.Stone;

            default:
                return 0;
        }
    }

    public void AddResource(
        ResourceType type,
        int amount)
    {
        if (amount <= 0)
            return;

        switch (type)
        {
            case ResourceType.Gold:
                Data.Water += amount;
                break;

            case ResourceType.Wood:
                Data.Wood += amount;
                break;

            case ResourceType.Stone:
                Data.Stone += amount;
                break;
        }
    }

    public bool HasResources(ResourceCost[] costs)
    {
        if (costs == null)
            return true;

        for (int i = 0; i < costs.Length; i++)
        {
            if (GetResource(costs[i].type) < costs[i].amount)
                return false;
        }

        return true;
    }

    public bool SpendResources(ResourceCost[] costs)
    {
        if (!HasResources(costs))
            return false;

        if (costs == null)
            return true;

        for (int i = 0; i < costs.Length; i++)
        {
            AddResource(
                costs[i].type,
                -costs[i].amount
            );
        }

        return true;
    }

    // -------------------------
    // 主城
    // -------------------------

    public void DamageMainCity(int damage)
    {
        if (damage <= 0)
            return;

        Data.MainCityHealth =
            Mathf.Max(
                0,
                Data.MainCityHealth - damage
            );
    }

    public void HealMainCity(int amount)
    {
        if (amount <= 0)
            return;

        Data.MainCityHealth =
            Mathf.Min(
                Data.MainCityMaxHealth,
                Data.MainCityHealth + amount
            );
    }

    public float GetMainCityHealthPercent()
    {
        if (Data.MainCityMaxHealth <= 0)
            return 0f;

        return (float)Data.MainCityHealth /
               Data.MainCityMaxHealth;
    }

    public bool IsMainCityDead()
    {
        return Data.MainCityHealth <= 0;
    }

    // -------------------------
    // 波次
    // -------------------------

    public void SetWave(int wave)
    {
        Data.CurrentWave =
            Mathf.Max(1, wave);
    }

    public void NextWave()
    {
        Data.CurrentWave++;
    }
}