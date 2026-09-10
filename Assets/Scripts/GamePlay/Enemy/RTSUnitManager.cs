using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RTSUnitManager : MonoBehaviour
{
    public static RTSUnitManager Instance { get; private set; }


    private readonly List<UnitMover> units =
        new();


    // 新增：待重算队列，配合 pendingSet 做去重，避免同一单位被重复入队
    private readonly Queue<UnitMover> pendingRetarget =
        new();

    private readonly HashSet<UnitMover> pendingSet =
        new();


    // 新增：每帧最多处理多少个单位的重算，用于把开销摊到多帧，需要根据实测调整
    [SerializeField]
    private int unitsPerFrame = 5;

    private Coroutine retargetRoutine;


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



    // 修改：不再订阅 BuildingManager.OnBuildingChanged（无参数，无法判断具体哪栋建筑变化），
    // 改为直接订阅 BuildingEvents.OnBuilt / OnDestroyed，两者都是静态事件，不依赖实例是否已创建
    private void OnEnable()
    {
        BuildingEvents.OnBuilt += HandleBuildingBuilt;
        BuildingEvents.OnDestroyed += HandleBuildingDestroyed;
    }



    private void OnDisable()
    {
        BuildingEvents.OnBuilt -= HandleBuildingBuilt;
        BuildingEvents.OnDestroyed -= HandleBuildingDestroyed;
    }



    private void HandleBuildingBuilt(Building building)
    {
        EvaluateUnits(building, false);
    }



    private void HandleBuildingDestroyed(Building building)
    {
        EvaluateUnits(building, true);
    }



    // 新增：过滤阶段，只做廉价距离比较（在 UnitMover.ShouldRetarget 内部），
    // 把真正需要重算的单位放入队列，不在这里做任何寻路相关的重计算
    private void EvaluateUnits(Building building, bool destroyed)
    {
        for (int i = 0; i < units.Count; i++)
        {
            UnitMover unit =
                units[i];

            if (unit == null)
                continue;

            if (pendingSet.Contains(unit))
                continue;

            if (unit.ShouldRetarget(building, destroyed))
            {
                pendingSet.Add(unit);
                pendingRetarget.Enqueue(unit);
            }
        }

        if (retargetRoutine == null)
        {
            retargetRoutine =
                StartCoroutine(
                    ProcessRetargetQueue()
                );
        }
    }



    // 新增：分帧执行阶段，每帧只处理 unitsPerFrame 个单位的真正重算
    private IEnumerator ProcessRetargetQueue()
    {
        while (pendingRetarget.Count > 0)
        {
            int processed = 0;

            while (processed < unitsPerFrame &&
                   pendingRetarget.Count > 0)
            {
                UnitMover unit =
                    pendingRetarget.Dequeue();

                pendingSet.Remove(unit);

                if (unit != null)
                {
                    unit.OnBuildingChanged();
                }

                processed++;
            }

            yield return null;
        }

        retargetRoutine = null;
    }



    public void Register(UnitMover unit)
    {
        if (!units.Contains(unit))
            units.Add(unit);
    }



    public void Unregister(UnitMover unit)
    {
        units.Remove(unit);

        // 新增：单位移除时同步清理待处理队列里的残留引用
        pendingSet.Remove(unit);
    }



    public List<UnitMover> GetUnits()
    {
        return units;
    }

    public UnitMover FindNearest(
     Vector2 position,
     float range)
    {
        if (units.Count == 0)
            return null;

        UnitMover nearest = null;

        float maxDistance = range * range;
        float minDistance = maxDistance;

        for (int i = 0; i < units.Count; i++)
        {
            UnitMover unit = units[i];

            if (unit == null)
                continue;

            float distance =
                ((Vector2)unit.transform.position - position)
                .sqrMagnitude;

            if (distance <= minDistance)
            {
                minDistance = distance;
                nearest = unit;
            }
        }

        return nearest;
    }
}