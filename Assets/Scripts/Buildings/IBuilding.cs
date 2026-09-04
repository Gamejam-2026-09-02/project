using UnityEngine;

public interface IBuilding
{
    BuildingData Data { get; }

    string Name { get; }

    string Description { get; }

    Vector2Int[] OccupiedCells { get; }

    ResourceCost[] Costs { get; }

    void Build(Vector2Int gridPosition);

    void Destroy();
}