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



        // 修改：起点不再只取最近一格，而是按距离升序依次尝试，
        // 跳过被占用（不可行走）或落在目标建筑占地内的格子，取第一个可行走的
        Vector2Int? startEdgeResult =
            GetClosestWalkableCell(
                startEdges,
                targetPosition,
                targetCells,
                map
            );

        if (startEdgeResult == null)
        {
            return null;
        }

        Vector2Int startEdge =
            startEdgeResult.Value;



        // 修改：终点同理，按距离升序依次尝试，跳过被占用或落在起始建筑占地内的格子
        Vector2Int? targetEdgeResult =
            GetClosestWalkableCell(
                targetEdges,
                startEdge,
                startCells,
                map
            );

        if (targetEdgeResult == null)
        {
            return null;
        }

        Vector2Int targetEdge =
            targetEdgeResult.Value;



        List<Vector2> path =
            BuildingPathfinder.FindPath(
                map.GridToWorld(startEdge),
                map.GridToWorld(targetEdge)
            );


        if (path == null ||
            path.Count == 0)
        {
            return null;
        }



        if (PathContainsCells(
            path,
            targetCells,
            map))
        {
            return null;
        }


        return path;
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
               grid,
               false   // 修改：根生成时静默，不触发连通性/单位重算
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



    // 新增：在候选格子集合中，按到 reference 的距离升序依次尝试，
    // 跳过落在 excludeCells 内的格子和不可行走的格子，返回第一个满足条件的格子。
    // 复杂度：O(K log K) 排序 + O(K) 遍历（K 为候选格子数，即建筑边缘格子数量）
    private Vector2Int? GetClosestWalkableCell(
        IEnumerable<Vector2Int> candidates,
        Vector2Int reference,
        HashSet<Vector2Int> excludeCells,
        GridMapManager map)
    {
        List<Vector2Int> sorted =
            new List<Vector2Int>();

        foreach (Vector2Int cell in candidates)
        {
            if (excludeCells.Contains(cell))
                continue;

            sorted.Add(cell);
        }

        sorted.Sort(
            (a, b) =>
            {
                int distA =
                    (a - reference).sqrMagnitude;

                int distB =
                    (b - reference).sqrMagnitude;

                return distA.CompareTo(distB);
            }
        );

        foreach (Vector2Int cell in sorted)
        {
            GridNode node =
                map.GetNode(cell);

            if (node != null &&
                node.Walkable)
            {
                return cell;
            }
        }

        return null;
    }
}