using System.Collections.Generic;
using UnityEngine;

public static class BuildingPathfinder
{
    private static readonly Vector2Int[] Directions =
    {
        new(1,0),
        new(-1,0),
        new(0,1),
        new(0,-1),

        new(1,1),
        new(1,-1),
        new(-1,1),
        new(-1,-1)
    };


    private static int searchId;


    public static List<Vector2> FindPath(
        Vector2 startWorld,
        Vector2 endWorld)
    {
        GridMapManager map =
            GridMapManager.Instance;


        if (map == null)
            return null;


        Vector2Int start =
            map.WorldToGrid(startWorld);


        Vector2Int end =
            map.WorldToGrid(endWorld);



        List<Vector2Int> gridPath =
            FindGridPath(
                map,
                start,
                end
            );


        if (gridPath == null)
            return null;



        // 保留完整连续格子
        gridPath =
            AddBuildingCorners(gridPath);



        List<Vector2> result =
            new(gridPath.Count);



        foreach (Vector2Int cell in gridPath)
        {
            result.Add(
                map.GridToWorld(cell)
            );
        }


        return result;
    }



    private static List<Vector2Int> FindGridPath(
        GridMapManager map,
        Vector2Int start,
        Vector2Int end)
    {
        GridNode startNode =
            map.GetNode(start);

        GridNode endNode =
            map.GetNode(end);



        if (startNode == null ||
           endNode == null)
            return null;


        if (!startNode.Walkable ||
           !endNode.Walkable)
            return null;



        searchId++;

        if (searchId == int.MaxValue)
            searchId = 1;



        List<GridNode> open =
            new();


        HashSet<GridNode> closed =
            new();



        InitializeNode(
            startNode,
            searchId
        );


        startNode.GCost = 0;

        startNode.HCost =
            GetDistance(
                start,
                end
            );


        startNode.Parent = null;


        open.Add(startNode);



        while (open.Count > 0)
        {
            GridNode current =
                open[0];


            for (int i = 1; i < open.Count; i++)
            {
                GridNode node =
                    open[i];


                if (node.FCost < current.FCost ||
                   node.FCost == current.FCost &&
                   node.HCost < current.HCost)
                {
                    current = node;
                }
            }



            open.Remove(current);

            closed.Add(current);



            if (current == endNode)
            {
                return BuildPath(endNode);
            }



            foreach (Vector2Int dir in Directions)
            {
                Vector2Int next =
                    current.Position + dir;



                GridNode nextNode =
                    map.GetNode(next);



                if (nextNode == null ||
                   !nextNode.Walkable ||
                   closed.Contains(nextNode))
                {
                    continue;
                }



                if (IsDiagonal(
                    current.Position,
                    next))
                {
                    if (!CanMoveDiagonal(
                        map,
                        current.Position,
                        next))
                    {
                        continue;
                    }
                }



                InitializeNode(
                    nextNode,
                    searchId
                );



                int moveCost =
                    IsDiagonal(
                        current.Position,
                        next)
                    ? 14
                    : 10;



                int newCost =
                    current.GCost +
                    moveCost;



                bool isNew =
                    !open.Contains(nextNode);



                if (isNew ||
                   newCost < nextNode.GCost)
                {
                    nextNode.GCost =
                        newCost;


                    nextNode.HCost =
                        GetDistance(
                            next,
                            end
                        );


                    nextNode.Parent =
                        current;


                    if (isNew)
                        open.Add(nextNode);
                }
            }
        }


        return null;
    }



    private static bool IsDiagonal(
        Vector2Int a,
        Vector2Int b)
    {
        return a.x != b.x &&
               a.y != b.y;
    }



    private static bool CanMoveDiagonal(
        GridMapManager map,
        Vector2Int from,
        Vector2Int to)
    {
        GridNode sideA =
            map.GetNode(
                new Vector2Int(
                    to.x,
                    from.y
                )
            );


        GridNode sideB =
            map.GetNode(
                new Vector2Int(
                    from.x,
                    to.y
                )
            );


        return sideA != null &&
               sideB != null &&
               sideA.Walkable &&
               sideB.Walkable;
    }



    private static void InitializeNode(
        GridNode node,
        int id)
    {
        if (node.SearchId == id)
            return;


        node.SearchId = id;

        node.GCost = 0;
        node.HCost = 0;
        node.Parent = null;
    }



    private static int GetDistance(
        Vector2Int a,
        Vector2Int b)
    {
        int dx =
            Mathf.Abs(
                a.x - b.x
            );

        int dy =
            Mathf.Abs(
                a.y - b.y
            );


        int diagonal =
            Mathf.Min(dx, dy);


        int straight =
            Mathf.Abs(dx - dy);



        return diagonal * 14 +
               straight * 10;
    }



    private static List<Vector2Int> BuildPath(
        GridNode end)
    {
        List<Vector2Int> result =
            new();


        GridNode current =
            end;


        while (current != null)
        {
            result.Add(
                current.Position
            );

            current =
                current.Parent;
        }


        result.Reverse();


        return result;
    }



    /// <summary>
    /// 增加建筑线路拐角
    /// 但不删除格子
    /// </summary>
    private static List<Vector2Int> AddBuildingCorners(
        List<Vector2Int> path)
    {
        if (path.Count <= 2)
            return path;



        List<Vector2Int> result =
            new();


        result.Add(path[0]);


        Vector2Int lastDir =
            path[1] - path[0];


        int straightCount = 0;



        for (int i = 1;
            i < path.Count - 1;
            i++)
        {
            Vector2Int dir =
                path[i + 1] -
                path[i];



            straightCount++;



            if (dir != lastDir)
            {
                result.Add(
                    path[i]
                );

                straightCount = 0;
            }
            else if (straightCount >= 3)
            {
                result.Add(
                    path[i]
                );

                straightCount = 0;
            }


            lastDir = dir;
        }



        result.Add(
            path[^1]
        );


        return ExpandPath(result);
    }



    /// <summary>
    /// 将控制点重新展开为连续格子
    /// </summary>
    private static List<Vector2Int> ExpandPath(
        List<Vector2Int> points)
    {
        List<Vector2Int> result =
            new();



        for (int i = 0;
            i < points.Count - 1;
            i++)
        {
            Vector2Int start =
                points[i];

            Vector2Int end =
                points[i + 1];



            int length =
                Mathf.Max(
                    Mathf.Abs(
                        end.x - start.x
                    ),
                    Mathf.Abs(
                        end.y - start.y
                    )
                );



            for (int j = 0;
                j < length;
                j++)
            {
                float t =
                    j / (float)length;



                Vector2Int cell =
                    Vector2Int.RoundToInt(
                        Vector2.Lerp(
                            start,
                            end,
                            t
                        )
                    );


                if (result.Count == 0 ||
                   result[^1] != cell)
                {
                    result.Add(cell);
                }
            }
        }



        if (result.Count == 0 ||
           result[^1] != points[^1])
        {
            result.Add(
                points[^1]
            );
        }


        return result;
    }
}