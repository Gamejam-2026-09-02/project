using System;
using UnityEngine;

[Serializable]
public class BuildingEntry
{
    public BuildingData data;

    [Tooltip("这个建筑是否属于 Root。")]
    public bool isRoot;
}


public class BuildingGenerator : MonoBehaviour
{
    public static BuildingGenerator Instance { get; private set; }


    [SerializeField]
    private BuildingEntry[] buildings;


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


    // =========================================================
    // 生成
    // =========================================================

    // 修改：新增 notify 参数并透传给 Build，默认 true 不影响现有调用方
    public Building Generate(
        BuildingData data,
        Vector2Int gridPosition,
        bool notify = true)
    {
        if (!CanGenerate(
                data,
                gridPosition))
        {
            return null;
        }

        if (data == null ||
            data.prefab == null)
        {
            Debug.LogError(
                $"建筑数据或预制体为空：{data?.Name}"
            );
            return null;
        }

        GridMapManager map =
            GridMapManager.Instance;

        GameObject obj =
            Instantiate(
                data.prefab,
                map.GridToWorld(gridPosition),
                Quaternion.identity
            );

        Building building =
            obj.GetComponent<Building>();

        if (building == null)
        {
            Debug.LogError(
                $"建筑预制体缺少 Building 组件：{data.Name}"
            );
            Destroy(obj);
            return null;
        }

        building.Initialize(
            IsRoot(data)
        );

        // 修改：透传 notify
        building.Build(
            gridPosition,
            notify
        );

        return building;
    }


    // =========================================================
    // 能否生成
    // =========================================================

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


        if (cells == null ||
            cells.Length == 0)
        {
            return false;
        }


        for (int i = 0; i < cells.Length; i++)
        {
            Vector2Int position =
                gridPosition + cells[i];


            GridNode node =
                map.GetNode(position);


            if (node == null)
                return false;


            if (!node.Walkable)
                return false;


            if (node.IsOccupied)
                return false;
        }


        return true;
    }


    // =========================================================
    // 获取预制体
    // =========================================================

    public Building GetPrefab(
        BuildingData data)
    {
        return data != null
            ? data.prefab.GetComponent<Building>()
            : null;
    }


    // =========================================================
    // Root判断
    // =========================================================

    public bool IsRoot(
        BuildingData data)
    {
        BuildingEntry entry =
            FindEntry(data);


        return entry != null &&
               entry.isRoot;
    }


    private BuildingEntry FindEntry(
        BuildingData data)
    {
        if (data == null ||
            buildings == null)
        {
            return null;
        }


        for (int i = 0; i < buildings.Length; i++)
        {
            BuildingEntry entry =
                buildings[i];


            if (entry == null)
                continue;


            if (entry.data == data)
            {
                return entry;
            }
        }


        return null;
    }
}