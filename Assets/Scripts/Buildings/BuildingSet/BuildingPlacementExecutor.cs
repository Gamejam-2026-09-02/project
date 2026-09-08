using System.Collections.Generic;
using UnityEngine;

// 执行建造，连接，消耗资源
public class BuildingPlacementExecutor
{
    private BuildingData rootData;

    private BuildingPlacementPath pathFinder;



    public BuildingPlacementExecutor(
        BuildingData rootData,
        BuildingPlacementPath pathFinder)
    {
        this.rootData = rootData;
        this.pathFinder = pathFinder;
    }



    public bool Generate(
        List<Vector2> path,
        Vector2Int position,
        BuildingData buildingData)
    {
        if (path == null ||
            path.Count == 0)
        {
            Debug.Log("建造失败: 无有效路径");
            return false;
        }

        if (!pathFinder.GenerateRoots(
            path,
            position,
            rootData))
        {
            Debug.Log("建造失败: Root生成失败");
            return false;
        }

        BuildingGenerator generator =
            BuildingGenerator.Instance;

        if (generator == null)
        {
            Debug.Log("建造失败: BuildingGenerator不存在");
            return false;
        }

        // 修改：目标建筑生成时也静默，避免和根一起造成中间态的重复广播
        Building building =
            generator.Generate(
                buildingData,
                position,
                false
            );

        if (building == null)
        {
            Debug.Log("建造失败: 建筑生成失败");
            return false;
        }

        ConsumeResource(
            buildingData,
            path
        );

        // 新增：整批（根 + 目标建筑）全部生成完毕后，统一广播一次
        BuildingEvents.NotifyBuilt(building);

        if (BuildingManager.Instance != null)
        {
            BuildingManager.Instance.NotifyChangedManually();
        }

        Debug.Log($"建造成功: {buildingData.name}");
        return true;
    }

    public bool Connect(
        Building start,
        Building target)
    {
        BuildingConnector connector =
            new BuildingConnector(
                rootData,
                pathFinder
            );

        bool result =
            connector.TryConnect(
                start,
                target
            );

        if (result)
        {
            Debug.Log($"连接成功: {start.name} -> {target.name}");

            // 新增：连接过程中生成的根都是静默的，这里统一触发一次
            BuildingEvents.NotifyBuilt(target);

            if (BuildingManager.Instance != null)
            {
                BuildingManager.Instance.NotifyChangedManually();
            }
        }
        else
        {
            Debug.Log($"连接失败: {start.name} -> {target.name}");
        }

        return result;
    }


    private void ConsumeResource(
        BuildingData buildingData,
        List<Vector2> path)
    {
        // 消耗目标建筑资源
        ConsumeCost(
            buildingData.Costs
        );


        // 消耗Root资源
        int rootCount =
            path.Count;


        for (int i = 0; i < rootCount; i++)
        {
            ConsumeCost(
                rootData.Costs
            );
        }
    }





    private void ConsumeCost(
        ResourceCost[] costs)
    {
        if (costs == null ||
           PlayerGameDataManager.Instance == null)
        {
            return;
        }



        foreach (ResourceCost cost in costs)
        {
            PlayerGameDataManager.Instance
                .AddResource(
                    cost.type,
                    -cost.amount
                );
        }
    }
}