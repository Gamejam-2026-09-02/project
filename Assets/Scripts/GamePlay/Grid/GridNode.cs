using UnityEngine;

public class GridNode
{
    public readonly Vector2Int Position;

    /// <summary>
    /// 地图本身是否可以通行。
    /// 墙、障碍物等设置为 false。
    /// </summary>
    public bool Walkable;

    /// <summary>
    /// 当前有多少对象占用这个格子。
    /// </summary>
    public int OccupantCount;

    public bool IsOccupied => OccupantCount > 0;

    // A* 数据
    public int SearchId;
    public int GCost;
    public int HCost;
    public GridNode Parent;

    public int FCost => GCost + HCost;

    public GridNode(Vector2Int position, bool walkable)
    {
        Position = position;
        Walkable = walkable;
    }
}