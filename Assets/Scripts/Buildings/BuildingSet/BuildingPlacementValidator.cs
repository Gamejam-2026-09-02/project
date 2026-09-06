using System.Collections.Generic;
using UnityEngine;


// 判断合法，路径检查，资源检查
public class BuildingPlacementValidator
{
    private BuildingPlacementPath pathFinder;

    private BuildingData rootData;


    public BuildingPlacementValidator(
        BuildingData rootData,
        BuildingPlacementPath pathFinder)
    {
        this.rootData = rootData;
        this.pathFinder = pathFinder;
    }



    public PlacementResult Validate(
        Building startBuilding,
        Vector2Int startGrid,
        Vector2Int targetGrid,
        Building targetBuilding,
        BuildingData buildingData)
    {
        List<Vector2> path;



        if (targetBuilding != null)
        {
            path =
                pathFinder.FindPath(
                    startBuilding,
                    startGrid,
                    targetBuilding.GridPosition,
                    targetBuilding.Data
                );


            bool valid =
                path != null &&
                path.Count > 0 &&
                pathFinder.CanGenerateRoots(
                    path,
                    targetBuilding.GridPosition,
                    rootData
                );


            return new PlacementResult(
                path,
                valid
            );
        }



        path =
            pathFinder.FindPath(
                startBuilding,
                startGrid,
                targetGrid,
                buildingData
            );



        bool canBuild =
            BuildingGenerator.Instance != null &&
            BuildingGenerator.Instance.CanGenerate(
                buildingData,
                targetGrid
            );



        bool canRoot =
            path != null &&
            path.Count > 0 &&
            pathFinder.CanGenerateRoots(
                path,
                targetGrid,
                rootData
            );



        bool enoughResource =
            CheckResource(
                buildingData,
                path
            );



        bool validResult =
            canBuild &&
            canRoot &&
            enoughResource;



        return new PlacementResult(
            path,
            validResult
        );
    }



    private bool CheckResource(
        BuildingData buildingData,
        List<Vector2> path)
    {
        Dictionary<ResourceType, int> cost =
            new Dictionary<ResourceType, int>();


        // 目标建筑资源
        AddCost(
            cost,
            buildingData.Costs
        );


        // Root资源
        int rootCount =
            path != null ? path.Count : 0;


        for (int i = 0; i < rootCount; i++)
        {
            AddCost(
                cost,
                rootData.Costs
            );
        }



        foreach (var item in cost)
        {
            int current =
                PlayerGameDataManager.Instance
                .GetResource(item.Key);


            if (current < item.Value)
            {
                Debug.Log(
                    $"资源不足: {item.Key} 当前:{current} 需要:{item.Value}"
                );

                return false;
            }
        }


        return true;
    }



    private void AddCost(
        Dictionary<ResourceType, int> total,
        ResourceCost[] costs)
    {
        if (costs == null)
            return;


        foreach (ResourceCost cost in costs)
        {
            if (!total.ContainsKey(cost.type))
            {
                total[cost.type] = 0;
            }


            total[cost.type] += cost.amount;
        }
    }
}