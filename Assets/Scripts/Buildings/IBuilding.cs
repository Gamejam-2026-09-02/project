using UnityEngine;

public interface IBuilding
{
    BuildingData Data { get; }
    bool IsRoot { get; }
    Vector2Int GridPosition { get; }
    string Name { get; }
    string Description { get; }
    Vector2Int[] OccupiedCells { get; }
    ResourceCost[] Costs { get; }

    // 修改：新增 notify 参数并给默认值，与 Building.Build 的实现签名保持一致
    void Build(Vector2Int gridPosition, bool notify = true);

    void Destroy();
}