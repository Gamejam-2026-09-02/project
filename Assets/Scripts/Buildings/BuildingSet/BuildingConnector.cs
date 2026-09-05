using System.Collections.Generic;
using UnityEngine;


public class BuildingConnector
{
    private readonly BuildingData rootData;

    private readonly BuildingPlacementPath pathFinder;



    public BuildingConnector(
        BuildingData rootData,
        BuildingPlacementPath pathFinder)
    {
        this.rootData = rootData;
        this.pathFinder = pathFinder;
    }




    public bool TryConnect(
        Building start,
        Building target)
    {
        if (start == null ||
           target == null)
        {
            return false;
        }



        List<Vector2> path =
            pathFinder.FindPath(
                start,
                start.GridPosition,
                target.GridPosition,
                target.Data
            );



        if (path == null ||
           path.Count == 0)
        {
            return false;
        }



        if (!pathFinder.CanGenerateRoots(
            path,
            target.GridPosition,
            rootData))
        {
            return false;
        }



        if (!pathFinder.GenerateRoots(
            path,
            target.GridPosition,
            rootData))
        {
            return false;
        }



        if (BuildingConnectionManager.Instance != null)
        {
            BuildingConnectionManager.Instance.Refresh();
        }



        return true;
    }
}