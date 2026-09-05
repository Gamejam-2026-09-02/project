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
            Debug.Log(
                "建造失败: 无有效路径"
            );

            return false;
        }



        if (!pathFinder.GenerateRoots(
            path,
            position,
            rootData))
        {
            Debug.Log(
                "建造失败: Root生成失败"
            );

            return false;
        }



        BuildingGenerator generator =
            BuildingGenerator.Instance;



        if (generator == null)
        {
            Debug.Log(
                "建造失败: BuildingGenerator不存在"
            );

            return false;
        }



        Building building =
            generator.Generate(
                buildingData,
                position
            );



        if (building == null)
        {
            Debug.Log(
                "建造失败: 建筑生成失败"
            );

            return false;
        }



        ConsumeResource(
            buildingData,
            path
        );



        Debug.Log(
            $"建造成功: {buildingData.name}"
        );


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
            Debug.Log(
                $"连接成功: {start.name} -> {target.name}"
            );
        }
        else
        {
            Debug.Log(
                $"连接失败: {start.name} -> {target.name}"
            );
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