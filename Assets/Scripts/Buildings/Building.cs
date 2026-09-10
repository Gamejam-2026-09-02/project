using System;
using UnityEngine;

public class Building : MonoBehaviour, IBuilding, IDamageable
{
    [SerializeField]
    private BuildingData data;

    private Vector2Int gridPosition;

    private bool isRoot;

    private bool initialized;

    private int currentHealth;

    private float resourceTimer;

    public int MaxHealth =>
        data != null
            ? data.MaxHP
            : 0;


    public BuildingData Data =>
        data;


    public bool IsRoot =>
        isRoot;


    public Vector2Int GridPosition =>
        gridPosition;


    public int CurrentHealth =>
        currentHealth;



    public string Name =>
        data != null
            ? data.Name
            : string.Empty;


    public string Description =>
        data != null
            ? data.Description
            : string.Empty;


    public BuildingType Type =>
        data != null
            ? data.Type
            : default;


    public Vector2Int[] OccupiedCells =>
        data != null
            ? data.GetOccupiedCells()
            : null;


    public ResourceCost[] Costs =>
        data != null
            ? data.Costs
            : null;



    public event Action<int> OnDamaged;

    public event Action OnDeath;

    private bool connectedToHome;


    public bool ConnectedToHome =>
        connectedToHome;

    public event Action OnDestroyed;

    // 新增：缓存 SpriteRenderer，避免 SetConnection 每次调用都 GetComponent
    private SpriteRenderer cachedRenderer;

    private void Awake()
    {
        // 新增：Awake 时缓存一次，Building 目前没有其他 Awake 逻辑
        cachedRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetConnection(bool value)
    {
        // 修改：值未变化时跳过，避免无意义的重复赋值和视觉刷新
        if (connectedToHome == value)
            return;

        connectedToHome = value;
        ApplyConnectionVisual();
    }

    private void ApplyConnectionVisual()
    {
        // 修改：使用缓存的 renderer，不再每次 GetComponent
        if (cachedRenderer == null)
            return;

        cachedRenderer.color =
            connectedToHome
            ? Color.white
            : Color.gray;
    }

    public virtual void Initialize(
        bool isRoot = false)
    {
        if (data == null)
        {
            Debug.LogError(
                $"{name} 没有设置BuildingData",
                this
            );

            return;
        }


        this.isRoot = isRoot;

        currentHealth =
            data.MaxHP;


        initialized = true;
    }

    private void Update()
    {
        if (!initialized)
            return;


        if (data == null)
            return;


        ResourceCost change =
            data.resourceChange;


        if (change.amount == 0)
            return;


        resourceTimer += Time.deltaTime;


        if (resourceTimer >= 1f)
        {
            resourceTimer = 0;

            PlayerGameDataManager.Instance
                ?.AddResource(
                    change.type,
                    change.amount
                );
        }
    }

    // 修改：新增 notify 参数，默认 true，不影响现有单独生成建筑（基地、敌方建筑）的调用
    public virtual void Build(
        Vector2Int gridPosition,
        bool notify = true)
    {
        if (!initialized)
        {
            Debug.LogError(
                $"{name} 尚未初始化。",
                this
            );
            return;
        }

        this.gridPosition =
            gridPosition;

        GridMapManager map =
            GridMapManager.Instance;

        if (map == null)
            return;

        Vector2Int[] cells =
            data.GetOccupiedCells();

        for (int i = 0; i < cells.Length; i++)
        {
            Vector2Int position =
                gridPosition + cells[i];

            if (!map.Occupy(
                position,
                this))
            {
                Debug.LogError(
                    $"{name} 占用格子失败:{position}",
                    this
                );
            }
        }

        transform.position =
            map.GridToWorld(
                gridPosition
            );

        // 修改：Register 始终执行（保证 buildings 列表正确），
        // 但是否广播 OnBuildingChanged 由 notify 控制
        if (BuildingManager.Instance != null)
        {
            BuildingManager.Instance.Register(this, notify);
        }

        // 修改：批量生成中间步骤（根）传 notify:false 时跳过广播
        if (notify)
        {
            BuildingEvents.NotifyBuilt(this);
        }
    }



    public virtual Vector2Int GetNearestWalkableCell(
        Vector2Int from)
    {
        GridMapManager map =
            GridMapManager.Instance;


        if (map == null ||
            data == null)
        {
            return from;
        }


        Vector2Int best =
            from;


        float bestDistance =
            float.MaxValue;



        Vector2Int size =
            data.GetSize();



        int maxRange =
            Mathf.Max(
                size.x,
                size.y
            )
            + 10;



        for (int x = -maxRange; x <= maxRange; x++)
        {
            for (int y = -maxRange; y <= maxRange; y++)
            {
                Vector2Int pos =
                    gridPosition +
                    new Vector2Int(x, y);



                if (!map.IsWalkable(pos))
                    continue;



                float distanceToBuilding =
                    GetDistanceToBuilding(pos);



                if (distanceToBuilding > 1.5f)
                    continue;



                float distance =
                    (pos - from)
                    .sqrMagnitude;



                if (distance < bestDistance)
                {
                    bestDistance = distance;

                    best = pos;
                }
            }
        }


        return best;
    }



    private float GetDistanceToBuilding(
        Vector2Int cell)
    {
        float min =
            float.MaxValue;


        Vector2Int[] occupied =
            data.GetOccupiedCells();



        for (int i = 0; i < occupied.Length; i++)
        {
            float d =
                Vector2Int.Distance(
                    cell,
                    gridPosition + occupied[i]
                );


            if (d < min)
                min = d;
        }


        return min;
    }



    // ============================
    // 战斗
    // ============================


    public virtual void TakeDamage(
        int damage)
    {
        if (damage <= 0)
            return;


        currentHealth -= damage;


        OnDamaged?.Invoke(damage);



        if (currentHealth <= 0)
        {
            OnDeath?.Invoke();

            Destroy();
        }
    }



    // ============================
    // 销毁
    // ============================


    public virtual void Destroy()
    {
        GridMapManager map =
            GridMapManager.Instance;



        if (map != null &&
            data != null)
        {
            Vector2Int[] cells =
                data.GetOccupiedCells();



            for (int i = 0; i < cells.Length; i++)
            {
                map.Release(
                    gridPosition + cells[i],
                    this
                );
            }
        }



        if (BuildingManager.Instance != null)
        {
            BuildingManager.Instance.Unregister(this);
        }

        BuildingEvents.NotifyDestroyed(this);

        OnDestroyed?.Invoke();

        Destroy(gameObject);
    }
}