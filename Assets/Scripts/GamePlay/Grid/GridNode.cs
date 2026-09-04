using UnityEngine;

public class GridNode
{
    public readonly Vector2Int Position;

    public bool Walkable;

    // 当前占用这个格子的建筑
    public Building Occupant { get; private set; }

    public bool IsOccupied =>
        Occupant != null;

    // A*
    public int SearchId;
    public int GCost;
    public int HCost;
    public GridNode Parent;

    public int FCost =>
        GCost + HCost;

    public GridNode(
        Vector2Int position,
        bool walkable)
    {
        Position = position;
        Walkable = walkable;
    }

    public void SetOccupant(Building building)
    {
        Occupant = building;
    }

    public void ClearOccupant(Building building)
    {
        if (Occupant == building)
        {
            Occupant = null;
        }
    }
}