using System.Collections.Generic;
using UnityEngine;

public static class AStarPathfinder
{
    private static readonly Vector2Int[] Directions =
    {
        new(1, 0),
        new(-1, 0),
        new(0, 1),
        new(0, -1)
    };

    private static int searchId;


    /// <summary>
    /// 从世界坐标寻找前往目标点的最短路径。
    /// 返回的路径为世界坐标。
    /// 不可达返回 null。
    /// </summary>
    public static List<Vector2> FindPath(
        Vector2 startWorld,
        Vector2 endWorld)
    {
        GridMapManager map = GridMapManager.Instance;

        if (map == null)
            return null;

        Vector2Int start = map.WorldToGrid(startWorld);
        Vector2Int end = map.WorldToGrid(endWorld);

        List<Vector2Int> gridPath = FindGridPath(
            map,
            start,
            end
        );

        if (gridPath == null)
            return null;

        List<Vector2> path = new(gridPath.Count);

        for (int i = 0; i < gridPath.Count; i++)
        {
            path.Add(
                map.GridToWorld(gridPath[i])
            );
        }

        return path;
    }


    /// <summary>
    /// A*网格寻路。
    /// </summary>
    private static List<Vector2Int> FindGridPath(
        GridMapManager map,
        Vector2Int start,
        Vector2Int end)
    {
        GridNode startNode = map.GetNode(start);
        GridNode endNode = map.GetNode(end);

        if (startNode == null || endNode == null)
            return null;

        if (!startNode.Walkable || !endNode.Walkable)
            return null;

        if (start == end)
        {
            return new List<Vector2Int>
            {
                start
            };
        }


        searchId++;

        // 防止 searchId 极端情况下溢出
        if (searchId == int.MaxValue)
            searchId = 1;


        List<GridNode> openList = new();
        HashSet<GridNode> closedSet = new();

        InitializeNode(startNode, searchId);

        startNode.GCost = 0;
        startNode.HCost = GetDistance(start, end);
        startNode.Parent = null;

        openList.Add(startNode);


        while (openList.Count > 0)
        {
            GridNode current = openList[0];

            // 找到 FCost 最低的节点
            // FCost相同时优先选择HCost低的节点
            for (int i = 1; i < openList.Count; i++)
            {
                GridNode node = openList[i];

                if (node.FCost < current.FCost ||
                    node.FCost == current.FCost &&
                    node.HCost < current.HCost)
                {
                    current = node;
                }
            }


            openList.Remove(current);
            closedSet.Add(current);


            if (current == endNode)
                return BuildPath(endNode);


            for (int i = 0; i < Directions.Length; i++)
            {
                Vector2Int nextPosition =
                    current.Position + Directions[i];

                GridNode nextNode =
                    map.GetNode(nextPosition);


                if (nextNode == null ||
                    !nextNode.Walkable ||
                    closedSet.Contains(nextNode))
                {
                    continue;
                }


                InitializeNode(nextNode, searchId);


                int newGCost =
                    current.GCost + 10;


                bool isNewNode =
                    !openList.Contains(nextNode);


                if (isNewNode ||
                    newGCost < nextNode.GCost)
                {
                    nextNode.GCost = newGCost;

                    nextNode.HCost =
                        GetDistance(
                            nextPosition,
                            end
                        );

                    nextNode.Parent = current;


                    if (isNewNode)
                        openList.Add(nextNode);
                }
            }
        }


        return null;
    }


    private static void InitializeNode(
        GridNode node,
        int currentSearchId)
    {
        if (node.SearchId == currentSearchId)
            return;

        node.SearchId = currentSearchId;
        node.GCost = 0;
        node.HCost = 0;
        node.Parent = null;
    }


    private static int GetDistance(
        Vector2Int a,
        Vector2Int b)
    {
        return
            (Mathf.Abs(a.x - b.x) +
             Mathf.Abs(a.y - b.y)) * 10;
    }


    private static List<Vector2Int> BuildPath(
        GridNode endNode)
    {
        List<Vector2Int> path = new();

        GridNode current = endNode;

        while (current != null)
        {
            path.Add(current.Position);
            current = current.Parent;
        }

        path.Reverse();

        return path;
    }
}