using System.Collections.Generic;
using UnityEngine;

public class BuildingPlacementPath
{
    public List<Vector2> FindPath(
        Building startBuilding,
        Vector2Int startPosition,
        Vector2Int targetPosition,
        BuildingData targetData)
    {
        GridMapManager map =
            GridMapManager.Instance;

        if (map == null ||
            startBuilding == null ||
            targetData == null)
        {
            return null;
        }


        HashSet<Vector2Int> targetCells =
            GetTargetBuildingCells(
                targetPosition,
                targetData
            );


        if (targetCells.Count == 0)
            return null;



        HashSet<Vector2Int> startCells =
            GetBuildingCells(
                startBuilding
            );


        if (startCells.Count == 0)
            return null;



        HashSet<Vector2Int> startEdges =
            GetEdgeCells(
                startCells
            );


        if (startEdges.Count == 0)
        {
            startEdges.Add(startPosition);
        }



        List<Vector2Int> targetEdges =
            GetEdgeList(
                targetCells
            );


        if (targetEdges.Count == 0)
            return null;



        List<Vector2> shortest =
            null;


        int shortestLength =
            int.MaxValue;



        foreach (Vector2Int startEdge in startEdges)
        {
            GridNode startNode =
                map.GetNode(startEdge);


            if (startNode == null ||
                !startNode.Walkable)
            {
                continue;
            }


            if (targetCells.Contains(startEdge))
                continue;



            foreach (Vector2Int targetEdge in targetEdges)
            {
                GridNode targetNode =
                    map.GetNode(targetEdge);


                if (targetNode == null ||
                    !targetNode.Walkable)
                {
                    continue;
                }


                if (targetCells.Contains(targetEdge))
                    continue;



                List<Vector2> path =
                    BuildingPathfinder.FindPath(
                        map.GridToWorld(startEdge),
                        map.GridToWorld(targetEdge)
                    );


                if (path == null ||
                    path.Count == 0)
                {
                    continue;
                }



                if (PathContainsCells(
                    path,
                    targetCells,
                    map))
                {
                    continue;
                }



                if (path.Count < shortestLength)
                {
                    shortestLength =
                        path.Count;

                    shortest =
                        path;
                }
            }
        }


        return shortest;
    }



    public bool CanGenerateRoots(
        List<Vector2> path,
        Vector2Int targetPosition,
        BuildingData rootData)
    {
        if (path == null ||
            rootData == null)
        {
            return false;
        }


        GridMapManager map =
            GridMapManager.Instance;


        BuildingGenerator generator =
            BuildingGenerator.Instance;


        if (map == null ||
            generator == null)
        {
            return false;
        }



        HashSet<Vector2Int> targetCells =
            GetTargetBuildingCells(
                targetPosition,
                rootData
            );



        HashSet<Vector2Int> checkedCells =
            new HashSet<Vector2Int>();


        for (int i = 0; i < path.Count; i++)
        {
            Vector2Int grid =
                map.WorldToGrid(
                    path[i]
                );


            if (targetCells.Contains(grid))
                continue;


            if (!checkedCells.Add(grid))
                continue;



            if (!generator.CanGenerate(
                rootData,
                grid))
            {
                return false;
            }
        }


        return true;
    }



    public bool GenerateRoots(
        List<Vector2> path,
        Vector2Int targetPosition,
        BuildingData rootData)
    {
        if (path == null ||
            rootData == null)
        {
            return false;
        }


        GridMapManager map =
            GridMapManager.Instance;


        BuildingGenerator generator =
            BuildingGenerator.Instance;


        if (map == null ||
            generator == null)
        {
            return false;
        }



        HashSet<Vector2Int> targetCells =
            GetTargetBuildingCells(
                targetPosition,
                rootData
            );



        HashSet<Vector2Int> generated =
            new HashSet<Vector2Int>();


        for (int i = 0; i < path.Count; i++)
        {
            Vector2Int grid =
                map.WorldToGrid(
                    path[i]
                );


            if (targetCells.Contains(grid))
                continue;


            if (!generated.Add(grid))
                continue;



            Building root =
                generator.Generate(
                    rootData,
                    grid
                );


            if (root == null)
                return false;
        }


        return true;
    }



    private HashSet<Vector2Int> GetBuildingCells(
        Building building)
    {
        HashSet<Vector2Int> result =
            new HashSet<Vector2Int>();


        if (building == null ||
            building.OccupiedCells == null)
        {
            return result;
        }



        Vector2Int center =
            building.GridPosition;


        foreach (Vector2Int offset in building.OccupiedCells)
        {
            result.Add(
                center + offset
            );
        }


        return result;
    }



    private HashSet<Vector2Int> GetTargetBuildingCells(
        Vector2Int center,
        BuildingData data)
    {
        HashSet<Vector2Int> result =
            new HashSet<Vector2Int>();


        Vector2Int[] cells =
            data.GetOccupiedCells();


        if (cells == null)
            return result;



        foreach (Vector2Int offset in cells)
        {
            result.Add(
                center + offset
            );
        }


        return result;
    }



    private HashSet<Vector2Int> GetEdgeCells(
        HashSet<Vector2Int> cells)
    {
        HashSet<Vector2Int> result =
            new HashSet<Vector2Int>();


        foreach (Vector2Int cell in cells)
        {
            AddOutsideCells(
                cell,
                cells,
                result
            );
        }


        return result;
    }



    private List<Vector2Int> GetEdgeList(
        HashSet<Vector2Int> cells)
    {
        HashSet<Vector2Int> set =
            GetEdgeCells(cells);


        return new List<Vector2Int>(set);
    }



    private void AddOutsideCells(
        Vector2Int cell,
        HashSet<Vector2Int> source,
        HashSet<Vector2Int> result)
    {
        Vector2Int[] directions =
        {
            Vector2Int.right,
            Vector2Int.left,
            Vector2Int.up,
            Vector2Int.down
        };


        foreach (Vector2Int dir in directions)
        {
            Vector2Int next =
                cell + dir;


            if (!source.Contains(next))
            {
                result.Add(next);
            }
        }
    }



    private bool PathContainsCells(
        List<Vector2> path,
        HashSet<Vector2Int> cells,
        GridMapManager map)
    {
        for (int i = 0; i < path.Count; i++)
        {
            Vector2Int grid =
                map.WorldToGrid(
                    path[i]
                );


            if (cells.Contains(grid))
                return true;
        }


        return false;
    }
}