using System.Collections.Generic;
using UnityEngine;

public static class AStarPathfinder
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

        // 转世界坐标
        List<Vector2> worldPath =
            new();


        for (int i = 0; i < gridPath.Count; i++)
        {
            worldPath.Add(
                map.GridToWorld(gridPath[i])
            );
        }

        return worldPath;
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
            GridNode current = open[0];


            for (int i = 1; i < open.Count; i++)
            {
                GridNode node = open[i];


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
                    continue;



                if (IsDiagonal(
                    current.Position,
                    next))
                {
                    if (!CanMoveDiagonal(
                        map,
                        current.Position,
                        next))
                        continue;
                }



                InitializeNode(
                    nextNode,
                    searchId
                );


                int cost =
                    IsDiagonal(
                        current.Position,
                        next)
                    ? 14
                    : 10;



                int newG =
                    current.GCost + cost;



                bool newNode =
                    !open.Contains(nextNode);



                if (newNode ||
                   newG < nextNode.GCost)
                {
                    nextNode.GCost = newG;

                    nextNode.HCost =
                        GetDistance(
                            next,
                            end
                        );


                    nextNode.Parent = current;


                    if (newNode)
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
        GridNode a =
            map.GetNode(
                new Vector2Int(
                    to.x,
                    from.y
                )
            );


        GridNode b =
            map.GetNode(
                new Vector2Int(
                    from.x,
                    to.y
                )
            );


        return a != null &&
               b != null &&
               a.Walkable &&
               b.Walkable;
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
            Mathf.Abs(a.x - b.x);

        int dy =
            Mathf.Abs(a.y - b.y);


        int diagonal =
            Mathf.Min(dx, dy);


        int straight =
            dx + dy - diagonal * 2;


        return diagonal * 14 +
               straight * 10;
    }



    private static List<Vector2Int> BuildPath(
        GridNode end)
    {
        List<Vector2Int> path =
            new();


        GridNode current = end;


        while (current != null)
        {
            path.Add(
                current.Position
            );

            current = current.Parent;
        }


        path.Reverse();


        return path;
    }

}