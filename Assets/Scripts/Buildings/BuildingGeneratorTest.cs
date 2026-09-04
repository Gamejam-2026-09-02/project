using System.Collections.Generic;
using UnityEngine;

public class BuildingGeneratorTest : MonoBehaviour
{
    private enum PreviewState
    {
        SelectStart,
        SelectTarget
    }

    [Header("建筑")]
    [SerializeField]
    private BuildingData buildingData;

    [Header("Root")]
    [SerializeField]
    private BuildingData rootBuildingData;

    [Header("预览")]
    [SerializeField, Range(0f, 1f)]
    private float previewAlpha = 0.5f;

    [SerializeField]
    private Color invalidColor = Color.red;

    [SerializeField]
    private int previewSortingOrder = 100;

    private PreviewState state =
        PreviewState.SelectStart;

    private Building previewBuilding;

    private readonly List<Building> previewRoots =
        new List<Building>();

    private List<Vector2> currentPath;

    private Building startBuilding;

    private Vector2Int startGridPosition;

    private Vector2Int previewGridPosition;

    private bool hasStart;

    private bool isPreviewValid;


    // =========================================================
    // Unity
    // =========================================================

    private void Start()
    {
        EnterSelectStart();
    }

    private void Update()
    {
        switch (state)
        {
            case PreviewState.SelectStart:
                UpdateSelectStart();
                break;

            case PreviewState.SelectTarget:
                UpdateSelectTarget();
                break;
        }
    }


    // =========================================================
    // 第一阶段：选择起点
    // =========================================================

    private void UpdateSelectStart()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TrySelectStart();
        }
    }

    private void TrySelectStart()
    {
        GridMapManager map =
            GridMapManager.Instance;

        if (map == null)
            return;

        Camera camera =
            Camera.main;

        if (camera == null)
            return;

        Vector3 worldPosition =
            camera.ScreenToWorldPoint(
                Input.mousePosition
            );

        Vector2Int gridPosition =
            map.WorldToGrid(
                worldPosition
            );

        GridNode node =
            map.GetNode(
                gridPosition
            );

        if (node == null)
            return;


        // -----------------------------------------------------
        // 必须点击 Occupied 格子
        // -----------------------------------------------------

        if (!node.IsOccupied)
        {
            Debug.Log(
                "起点必须选择已经被占用的格子。"
            );

            return;
        }


        // -----------------------------------------------------
        // 直接取得这个格子所属的 Building
        // -----------------------------------------------------

        Building building =
            node.Occupant;

        if (building == null)
            return;


        startBuilding =
            building;

        startGridPosition =
            gridPosition;

        hasStart =
            true;


        Debug.Log(
            $"Root 起点：" +
            $"{building.Name} " +
            $"类型：{(building.IsRoot ? "Root" : "Building")} " +
            $"点击位置：{gridPosition}"
        );


        EnterSelectTarget();
    }


    // =========================================================
    // 第二阶段
    // =========================================================

    private void EnterSelectTarget()
    {
        state =
            PreviewState.SelectTarget;

        CreateBuildingPreview();
    }

    private void SetPreviewSortingOrder(GameObject target)
    {
        if (target == null)
            return;

        SpriteRenderer[] sprites =
            target.GetComponentsInChildren<SpriteRenderer>(true);

        for (int i = 0; i < sprites.Length; i++)
        {
            sprites[i].sortingOrder = previewSortingOrder;
        }
    }

    private void UpdateSelectTarget()
    {
        UpdatePreview();


        // 左键生成
        if (Input.GetMouseButtonDown(0))
        {
            TryGenerate();
        }


        // 右键 / ESC
        // 回到重新选择起点
        if (Input.GetMouseButtonDown(1) ||
            Input.GetKeyDown(KeyCode.Escape))
        {
            EnterSelectStart();
        }
    }


    // =========================================================
    // 创建建筑预览
    // =========================================================

    private void CreateBuildingPreview()
    {
        ClearPreview();

        if (buildingData == null)
            return;

        BuildingGenerator generator =
            BuildingGenerator.Instance;

        if (generator == null)
        {
            Debug.LogError(
                "没有找到 BuildingGenerator。"
            );

            return;
        }

        Building prefab =
            generator.GetPrefab(
                buildingData
            );

        if (prefab == null)
        {
            Debug.LogError(
                $"找不到建筑预制体：" +
                $"{buildingData.Name}"
            );

            return;
        }

        previewBuilding = Instantiate(prefab);

        previewBuilding.name = $"{prefab.name}_Preview";

        DisablePreviewComponents(
            previewBuilding.gameObject
        );

        SetPreviewSortingOrder(
            previewBuilding.gameObject
        );

        SetColor(
            previewBuilding.gameObject,
            Color.white,
            previewAlpha
        );
    }


    // =========================================================
    // 更新预览
    // =========================================================

    private void UpdatePreview()
    {
        if (previewBuilding == null ||
            !hasStart)
        {
            return;
        }

        GridMapManager map =
            GridMapManager.Instance;

        if (map == null)
            return;

        Camera camera =
            Camera.main;

        if (camera == null)
            return;

        Vector3 worldPosition =
            camera.ScreenToWorldPoint(
                Input.mousePosition
            );

        previewGridPosition =
            map.WorldToGrid(
                worldPosition
            );

        previewBuilding.transform.position =
            map.GridToWorld(
                previewGridPosition
            );


        // -----------------------------------------------------
        // 建筑本身
        // -----------------------------------------------------

        BuildingGenerator generator =
            BuildingGenerator.Instance;

        if (generator == null)
            return;

        bool canGenerateBuilding =
            generator.CanGenerate(
                buildingData,
                previewGridPosition
            );


        // -----------------------------------------------------
        // 路径
        // -----------------------------------------------------

        currentPath =
            FindPathFromStart(
                previewGridPosition
            );

        bool hasPath =
            currentPath != null &&
            currentPath.Count > 0;


        // -----------------------------------------------------
        // Root
        // -----------------------------------------------------

        bool canGenerateRoot =
            hasPath &&
            CanGenerateRoots(
                currentPath
            );


        // -----------------------------------------------------
        // 最终状态
        // -----------------------------------------------------

        isPreviewValid =
            canGenerateBuilding &&
            hasPath &&
            canGenerateRoot;


        if (isPreviewValid)
        {
            SetColor(
                previewBuilding.gameObject,
                Color.white,
                previewAlpha
            );
        }
        else
        {
            SetColor(
                previewBuilding.gameObject,
                invalidColor,
                previewAlpha
            );
        }


        UpdateRootPreview();
    }


    // =========================================================
    // 寻找路径
    // =========================================================

    private List<Vector2> FindPathFromStart(
        Vector2Int target)
    {
        GridMapManager map =
            GridMapManager.Instance;

        if (map == null)
            return null;

        if (!hasStart ||
            startBuilding == null)
        {
            return null;
        }

        if (buildingData == null)
            return null;


        // -----------------------------------------------------
        // 目标建筑区域
        // -----------------------------------------------------

        HashSet<Vector2Int> targetCells =
            GetTargetBuildingCells(
                target
            );

        if (targetCells.Count == 0)
            return null;


        // -----------------------------------------------------
        // 获取起点建筑的所有占用格
        // -----------------------------------------------------

        HashSet<Vector2Int> startCells =
            GetBuildingCells(
                startBuilding
            );

        if (startCells.Count == 0)
            return null;


        // -----------------------------------------------------
        // 获取起点建筑外围
        // -----------------------------------------------------

        HashSet<Vector2Int> startEdges =
            GetEdgeCells(
                startCells
            );


        if (startEdges.Count == 0)
        {
            startEdges.Add(
                startGridPosition
            );
        }


        // -----------------------------------------------------
        // 目标建筑外围
        // -----------------------------------------------------

        List<Vector2Int> targetEdges =
            GetTargetEdgeCells(
                targetCells
            );

        if (targetEdges.Count == 0)
            return null;


        // -----------------------------------------------------
        // 起点外围 → 目标外围
        // -----------------------------------------------------

        List<Vector2> shortestPath =
            null;

        int shortestLength =
            int.MaxValue;


        foreach (Vector2Int startEdge
                 in startEdges)
        {
            GridNode startNode =
                map.GetNode(
                    startEdge
                );

            if (startNode == null)
                continue;

            if (!startNode.Walkable)
                continue;


            // 起点不能跑到目标建筑内部
            if (targetCells.Contains(
                    startEdge))
            {
                continue;
            }


            foreach (Vector2Int targetEdge
                     in targetEdges)
            {
                GridNode targetNode =
                    map.GetNode(
                        targetEdge
                    );

                if (targetNode == null)
                    continue;

                if (!targetNode.Walkable)
                    continue;


                if (targetCells.Contains(
                        targetEdge))
                {
                    continue;
                }


                List<Vector2> path =
                    AStarPathfinder.FindPath(
                        map.GridToWorld(
                            startEdge
                        ),
                        map.GridToWorld(
                            targetEdge
                        )
                    );


                if (path == null ||
                    path.Count == 0)
                {
                    continue;
                }


                // 不允许进入目标建筑
                if (PathContainsCells(
                        path,
                        targetCells,
                        map))
                {
                    continue;
                }


                if (path.Count <
                    shortestLength)
                {
                    shortestLength =
                        path.Count;

                    shortestPath =
                        path;
                }
            }
        }


        return shortestPath;
    }


    // =========================================================
    // 获取 Building 的占地
    // =========================================================

    private HashSet<Vector2Int> GetBuildingCells(
        Building building)
    {
        HashSet<Vector2Int> result =
            new HashSet<Vector2Int>();

        if (building == null)
            return result;

        Vector2Int[] cells =
            building.OccupiedCells;

        if (cells == null)
            return result;


        Vector2Int center =
            building.GridPosition;


        for (int i = 0; i < cells.Length; i++)
        {
            result.Add(
                center + cells[i]
            );
        }


        return result;
    }


    // =========================================================
    // 获取目标建筑占地
    // =========================================================

    private HashSet<Vector2Int> GetTargetBuildingCells(
        Vector2Int center)
    {
        HashSet<Vector2Int> cells =
            new HashSet<Vector2Int>();

        if (buildingData == null)
            return cells;

        Vector2Int[] occupiedCells =
            buildingData.GetOccupiedCells();

        if (occupiedCells == null)
            return cells;


        for (int i = 0;
             i < occupiedCells.Length;
             i++)
        {
            cells.Add(
                center +
                occupiedCells[i]
            );
        }


        return cells;
    }


    // =========================================================
    // 获取一个区域外围
    // =========================================================

    private HashSet<Vector2Int> GetEdgeCells(
        HashSet<Vector2Int> cells)
    {
        HashSet<Vector2Int> edgeCells =
            new HashSet<Vector2Int>();

        if (cells == null ||
            cells.Count == 0)
        {
            return edgeCells;
        }


        Vector2Int[] directions =
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1)
        };


        foreach (Vector2Int cell in cells)
        {
            for (int i = 0;
                 i < directions.Length;
                 i++)
            {
                Vector2Int outside =
                    cell + directions[i];


                if (cells.Contains(
                        outside))
                {
                    continue;
                }


                edgeCells.Add(
                    outside
                );
            }
        }


        return edgeCells;
    }


    // =========================================================
    // 获取目标建筑外围
    // =========================================================

    private List<Vector2Int> GetTargetEdgeCells(
        HashSet<Vector2Int> targetCells)
    {
        List<Vector2Int> result =
            new List<Vector2Int>();

        HashSet<Vector2Int> edgeSet =
            new HashSet<Vector2Int>();


        Vector2Int[] directions =
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1)
        };


        foreach (Vector2Int cell
                 in targetCells)
        {
            for (int i = 0;
                 i < directions.Length;
                 i++)
            {
                Vector2Int outside =
                    cell + directions[i];


                if (targetCells.Contains(
                        outside))
                {
                    continue;
                }


                if (edgeSet.Add(
                        outside))
                {
                    result.Add(
                        outside
                    );
                }
            }
        }


        return result;
    }


    // =========================================================
    // 路径是否进入指定区域
    // =========================================================

    private bool PathContainsCells(
        List<Vector2> path,
        HashSet<Vector2Int> cells,
        GridMapManager map)
    {
        for (int i = 0;
             i < path.Count;
             i++)
        {
            Vector2Int gridPosition =
                map.WorldToGrid(
                    path[i]
                );


            if (cells.Contains(
                    gridPosition))
            {
                return true;
            }
        }


        return false;
    }


    // =========================================================
    // Root 预览
    // =========================================================

    private void UpdateRootPreview()
    {
        ClearRootPreview();


        if (currentPath == null ||
            currentPath.Count == 0)
        {
            return;
        }


        if (rootBuildingData == null)
            return;


        GridMapManager map =
            GridMapManager.Instance;

        BuildingGenerator generator =
            BuildingGenerator.Instance;


        if (map == null ||
            generator == null)
        {
            return;
        }


        Building prefab =
            generator.GetPrefab(
                rootBuildingData
            );


        if (prefab == null)
        {
            Debug.LogError(
                $"找不到 Root 建筑预制体：" +
                $"{rootBuildingData.Name}"
            );

            return;
        }


        HashSet<Vector2Int> targetCells =
            GetTargetBuildingCells(
                previewGridPosition
            );


        HashSet<Vector2Int> generatedCells =
            new HashSet<Vector2Int>();


        for (int i = 0;
             i < currentPath.Count;
             i++)
        {
            Vector2Int gridPosition =
                map.WorldToGrid(
                    currentPath[i]
                );


            // 目标建筑内部不生成 Root
            if (targetCells.Contains(
                    gridPosition))
            {
                continue;
            }


            if (!generatedCells.Add(
                    gridPosition))
            {
                continue;
            }


            Building root =
       Instantiate(
           prefab,
           map.GridToWorld(gridPosition),
           Quaternion.identity
       );

            root.name =
                $"Root_Preview_{gridPosition.x}_{gridPosition.y}";

            DisablePreviewComponents(
                root.gameObject
            );

            SetPreviewSortingOrder(
                root.gameObject
            );

            SetColor(
                root.gameObject,
                isPreviewValid
                    ? Color.white
                    : invalidColor,
                previewAlpha
            );

            previewRoots.Add(root);
        }
    }


    // =========================================================
    // 检查 Root
    // =========================================================

    private bool CanGenerateRoots(
        List<Vector2> path)
    {
        if (rootBuildingData == null)
            return false;


        BuildingGenerator generator =
            BuildingGenerator.Instance;

        GridMapManager map =
            GridMapManager.Instance;


        if (generator == null ||
            map == null)
        {
            return false;
        }


        HashSet<Vector2Int> targetCells =
            GetTargetBuildingCells(
                previewGridPosition
            );


        HashSet<Vector2Int> checkedCells =
            new HashSet<Vector2Int>();


        for (int i = 0;
             i < path.Count;
             i++)
        {
            Vector2Int gridPosition =
                map.WorldToGrid(
                    path[i]
                );


            if (targetCells.Contains(
                    gridPosition))
            {
                continue;
            }


            if (!checkedCells.Add(
                    gridPosition))
            {
                continue;
            }


            if (!generator.CanGenerate(
                    rootBuildingData,
                    gridPosition))
            {
                return false;
            }
        }


        return true;
    }


    // =========================================================
    // 正式生成
    // =========================================================

    private void TryGenerate()
    {
        if (!isPreviewValid)
        {
            Debug.Log(
                "当前位置无法生成建筑。"
            );

            return;
        }


        if (buildingData == null ||
            rootBuildingData == null)
        {
            return;
        }


        BuildingGenerator generator =
            BuildingGenerator.Instance;

        GridMapManager map =
            GridMapManager.Instance;


        if (generator == null ||
            map == null)
        {
            return;
        }


        List<Vector2> path =
            FindPathFromStart(
                previewGridPosition
            );


        if (path == null ||
            path.Count == 0)
        {
            return;
        }


        if (!generator.CanGenerate(
                buildingData,
                previewGridPosition))
        {
            return;
        }


        if (!CanGenerateRoots(
                path))
        {
            return;
        }


        // -----------------------------------------------------
        // 先生成 Root
        // -----------------------------------------------------

        if (!GenerateRoots(
                path))
        {
            Debug.Log(
                "Root 生成失败。"
            );

            return;
        }


        // -----------------------------------------------------
        // 再生成目标建筑
        // -----------------------------------------------------

        Building building =
            generator.Generate(
                buildingData,
                previewGridPosition
            );


        if (building == null)
        {
            Debug.LogError(
                "建筑生成失败。"
            );

            return;
        }


        Debug.Log(
            $"建筑生成成功：" +
            $"{buildingData.Name}，" +
            $"起点建筑：" +
            $"{startBuilding.Name}，" +
            $"起点类型：" +
            $"{(startBuilding.IsRoot ? "Root" : "Building")}，" +
            $"目标：" +
            $"{previewGridPosition}，" +
            $"路径长度：" +
            $"{path.Count}"
        );


        // 生成完成后重新选择起点
        EnterSelectStart();
    }


    // =========================================================
    // 生成 Root
    // =========================================================

    private bool GenerateRoots(
        List<Vector2> path)
    {
        if (rootBuildingData == null)
            return false;


        BuildingGenerator generator =
            BuildingGenerator.Instance;

        GridMapManager map =
            GridMapManager.Instance;


        if (generator == null ||
            map == null)
        {
            return false;
        }


        HashSet<Vector2Int> targetCells =
            GetTargetBuildingCells(
                previewGridPosition
            );


        HashSet<Vector2Int> generatedCells =
            new HashSet<Vector2Int>();


        for (int i = 0;
             i < path.Count;
             i++)
        {
            Vector2Int gridPosition =
                map.WorldToGrid(
                    path[i]
                );


            if (targetCells.Contains(
                    gridPosition))
            {
                continue;
            }


            if (!generatedCells.Add(
                    gridPosition))
            {
                continue;
            }


            Building root =
                generator.Generate(
                    rootBuildingData,
                    gridPosition
                );


            if (root == null)
            {
                return false;
            }
        }


        return true;
    }


    // =========================================================
    // 回到选择起点
    // =========================================================

    private void EnterSelectStart()
    {
        state =
            PreviewState.SelectStart;

        hasStart =
            false;

        startBuilding =
            null;

        currentPath =
            null;

        isPreviewValid =
            false;

        ClearPreview();
    }


    // =========================================================
    // 清理
    // =========================================================

    private void ClearPreview()
    {
        if (previewBuilding != null)
        {
            Destroy(
                previewBuilding.gameObject
            );

            previewBuilding = null;
        }


        ClearRootPreview();

        currentPath = null;
    }


    private void ClearRootPreview()
    {
        for (int i = 0;
             i < previewRoots.Count;
             i++)
        {
            if (previewRoots[i] != null)
            {
                Destroy(
                    previewRoots[i].gameObject
                );
            }
        }


        previewRoots.Clear();
    }


    // =========================================================
    // 预览辅助
    // =========================================================

    private void DisablePreviewComponents(
        GameObject target)
    {
        if (target == null)
            return;


        Collider2D[] colliders =
            target.GetComponentsInChildren<Collider2D>(
                true
            );


        for (int i = 0;
             i < colliders.Length;
             i++)
        {
            colliders[i].enabled = false;
        }


        MonoBehaviour[] behaviours =
            target.GetComponentsInChildren<MonoBehaviour>(
                true
            );


        for (int i = 0;
             i < behaviours.Length;
             i++)
        {
            behaviours[i].enabled = false;
        }
    }


    private void SetColor(
        GameObject target,
        Color color,
        float alpha)
    {
        if (target == null)
            return;


        SpriteRenderer[] sprites =
            target.GetComponentsInChildren<SpriteRenderer>(
                true
            );


        for (int i = 0;
             i < sprites.Length;
             i++)
        {
            Color finalColor =
                color;

            finalColor.a =
                alpha;

            sprites[i].color =
                finalColor;
        }


        Renderer[] renderers =
            target.GetComponentsInChildren<Renderer>(
                true
            );


        for (int i = 0;
             i < renderers.Length;
             i++)
        {
            if (renderers[i] is SpriteRenderer)
                continue;


            Material material =
                renderers[i].material;


            if (material == null)
                continue;


            if (material.HasProperty(
                    "_Color"))
            {
                Color finalColor =
                    color;

                finalColor.a =
                    alpha;

                material.color =
                    finalColor;
            }
        }
    }


    private void OnDestroy()
    {
        ClearPreview();
    }
}