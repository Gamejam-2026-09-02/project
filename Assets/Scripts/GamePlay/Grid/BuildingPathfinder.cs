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

    // 修改：open 改为静态复用的二叉堆数组，避免每次调用分配 List/HashSet
    private static readonly List<GridNode> openHeap = new();
    private static readonly HashSet<GridNode> closed = new();

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

        // 修改：复用静态容器，每次搜索前清空而不是重新分配
        openHeap.Clear();
        closed.Clear();

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

        HeapPush(startNode);

        while (openHeap.Count > 0)
        {
            // 修改：O(log n) 弹出最小节点，替代原来的 O(n) 线性扫描
            GridNode current =
                HeapPop();

            // 修改：懒删除处理，跳过已经被更优路径处理过的重复节点
            if (closed.Contains(current))
                continue;

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

                // 修改：不再需要 isNew/Contains 判断，
                // 因为 GCost 在 InitializeNode 中已重置为“无穷大”，
                // 只要新路径更优就直接更新并压入堆
                if (newCost < nextNode.GCost)
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

                    HeapPush(nextNode);
                }
            }
        }

        return null;
    }

    // 新增：二叉堆 push，O(log n)
    private static void HeapPush(GridNode node)
    {
        openHeap.Add(node);

        int i = openHeap.Count - 1;

        while (i > 0)
        {
            int parent = (i - 1) / 2;

            if (Compare(openHeap[i], openHeap[parent]) < 0)
            {
                (openHeap[i], openHeap[parent]) =
                    (openHeap[parent], openHeap[i]);

                i = parent;
            }
            else
            {
                break;
            }
        }
    }

    // 新增：二叉堆 pop，O(log n)
    private static GridNode HeapPop()
    {
        GridNode root = openHeap[0];

        int last = openHeap.Count - 1;

        openHeap[0] = openHeap[last];
        openHeap.RemoveAt(last);

        int i = 0;
        int count = openHeap.Count;

        while (true)
        {
            int left = i * 2 + 1;
            int right = i * 2 + 2;
            int smallest = i;

            if (left < count &&
                Compare(openHeap[left], openHeap[smallest]) < 0)
            {
                smallest = left;
            }

            if (right < count &&
                Compare(openHeap[right], openHeap[smallest]) < 0)
            {
                smallest = right;
            }

            if (smallest == i)
                break;

            (openHeap[i], openHeap[smallest]) =
                (openHeap[smallest], openHeap[i]);

            i = smallest;
        }

        return root;
    }

    // 新增：堆排序比较规则，与原线性扫描的比较逻辑保持一致（FCost 优先，HCost 次之）
    private static int Compare(GridNode a, GridNode b)
    {
        if (a.FCost != b.FCost)
            return a.FCost.CompareTo(b.FCost);

        return a.HCost.CompareTo(b.HCost);
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

        // 修改：GCost 初始化为“无穷大”而不是 0，
        // 因为堆的懒删除方式不再依赖 open.Contains 判断是否为新节点，
        // 而是直接靠 GCost 比较决定是否需要松弛更新
        node.GCost = int.MaxValue;
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