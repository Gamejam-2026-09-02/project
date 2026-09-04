using UnityEngine;

public class Building : MonoBehaviour, IBuilding
{
    private BuildingData data;

    private Vector2Int gridPosition;


    public BuildingData Data => data;

    public string Name =>
        data != null ? data.Name : string.Empty;

    public string Description =>
        data != null ? data.Description : string.Empty;

    public Vector2Int[] OccupiedCells =>
        data != null
            ? data.GetOccupiedCells()
            : null;

    public ResourceCost[] Costs =>
        data != null
            ? data.Costs
            : null;


    /// <summary>
    /// 由建造系统在创建建筑后调用。
    /// </summary>
    public void Initialize(BuildingData data)
    {
        if (data == null)
        {
            Debug.LogError(
                $"{name} 初始化失败：BuildingData 为空。",
                this
            );

            return;
        }

        this.data = data;
    }


    public virtual void Build(Vector2Int gridPosition)
    {
        if (data == null)
        {
            Debug.LogError(
                $"{name} 尚未初始化 BuildingData。",
                this
            );

            return;
        }

        this.gridPosition = gridPosition;

        GridMapManager map =
            GridMapManager.Instance;

        if (map == null)
            return;

        Vector2Int[] cells =
            data.GetOccupiedCells();

        for (int i = 0; i < cells.Length; i++)
        {
            map.Occupy(
                gridPosition + cells[i]
            );
        }

        transform.position =
            map.GridToWorld(gridPosition);
    }


    public virtual void Destroy()
    {
        GridMapManager map =
            GridMapManager.Instance;

        if (map != null && data != null)
        {
            Vector2Int[] cells =
                data.GetOccupiedCells();

            for (int i = 0; i < cells.Length; i++)
            {
                map.Release(
                    gridPosition + cells[i]
                );
            }
        }

        Destroy(gameObject);
    }
}