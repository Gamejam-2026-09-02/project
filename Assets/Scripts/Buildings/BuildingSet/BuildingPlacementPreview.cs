using System.Collections.Generic;
using UnityEngine;


public class BuildingPlacementPreview
{
    private BuildingData buildingData;
    private readonly BuildingData rootBuildingData;


    private readonly float previewAlpha = 0.5f;
    private readonly Color invalidColor = Color.red;
    private readonly int previewSortingOrder = 100;


    private Building previewBuilding;

    // [修改] 用字典替代 List,按格子坐标索引,支持增量更新而非全量重建
    private readonly Dictionary<Vector2Int, Building> previewRootMap =
        new Dictionary<Vector2Int, Building>();

    // [修改] 复用临时集合,避免 ShowRoots 每次调用都 new HashSet
    private readonly HashSet<Vector2Int> reusableTargetCells =
        new HashSet<Vector2Int>();
    private readonly HashSet<Vector2Int> reusablePathCells =
        new HashSet<Vector2Int>();
    private readonly List<Vector2Int> reusableRemoveKeys =
        new List<Vector2Int>();



    public BuildingPlacementPreview(
        BuildingData buildingData,
        BuildingData rootBuildingData)
    {
        this.buildingData = buildingData;
        this.rootBuildingData = rootBuildingData;
    }

    public void SetBuildingData(
    BuildingData data)
    {
        if (buildingData == data)
            return;


        buildingData = data;


        Clear();
    }


    public void Create()
    {
        Clear();


        if (buildingData == null)
            return;


        BuildingGenerator generator =
            BuildingGenerator.Instance;


        if (generator == null)
            return;



        Building prefab =
            generator.GetPrefab(
                buildingData
            );


        if (prefab == null)
            return;



        previewBuilding =
            Object.Instantiate(prefab);


        previewBuilding.name =
            $"{prefab.name}_Preview";



        DisablePreviewComponents(
            previewBuilding.gameObject
        );


        SetPreviewSortingOrder(
            previewBuilding.gameObject
        );


        ResetColor();
    }




    public void SetPosition(
        Vector2Int gridPosition)
    {
        if (previewBuilding == null)
            return;


        GridMapManager map =
            GridMapManager.Instance;


        if (map == null)
            return;



        previewBuilding.transform.position =
            map.GridToWorld(
                gridPosition
            );
    }




    public void SetValid(
        bool valid)
    {
        if (previewBuilding == null)
            return;


        SetColor(
            previewBuilding.gameObject,
            valid
                ? Color.white
                : invalidColor
        );
    }




    public void ResetColor()
    {
        if (previewBuilding == null)
            return;


        SetColor(
            previewBuilding.gameObject,
            Color.white
        );
    }




    public void SetBuildingPreviewVisible(
        bool value)
    {
        if (previewBuilding == null)
            return;


        previewBuilding.gameObject.SetActive(
            value
        );
    }




    // [修改] 整个方法从"全量销毁重建"改为"增量 diff 更新"
    public void ShowRoots(
        List<Vector2> path,
        Vector2Int targetPosition,
        bool valid)
    {
        if (path == null ||
            path.Count == 0 ||
            rootBuildingData == null)
        {
            ClearRootPreview();
            return;
        }


        GridMapManager map =
            GridMapManager.Instance;

        BuildingGenerator generator =
            BuildingGenerator.Instance;

        if (map == null ||
            generator == null)
        {
            ClearRootPreview();
            return;
        }

        Building prefab =
            generator.GetPrefab(
                rootBuildingData
            );

        if (prefab == null)
        {
            ClearRootPreview();
            return;
        }


        // 计算目标位置占用格,用于路径排除
        reusableTargetCells.Clear();
        GetTargetCells(targetPosition, reusableTargetCells);

        // 计算本次需要显示的格子集合
        reusablePathCells.Clear();

        for (int i = 0; i < path.Count; i++)
        {
            Vector2Int grid = map.WorldToGrid(path[i]);

            if (reusableTargetCells.Contains(grid))
                continue;

            reusablePathCells.Add(grid);
        }


        // 移除不再需要的格子(旧格子里不在新集合中的)
        reusableRemoveKeys.Clear();

        foreach (KeyValuePair<Vector2Int, Building> pair in previewRootMap)
        {
            if (!reusablePathCells.Contains(pair.Key))
            {
                reusableRemoveKeys.Add(pair.Key);
            }
        }

        for (int i = 0; i < reusableRemoveKeys.Count; i++)
        {
            Vector2Int key = reusableRemoveKeys[i];

            if (previewRootMap.TryGetValue(key, out Building old) && old != null)
            {
                Object.Destroy(old.gameObject);
            }

            previewRootMap.Remove(key);
        }


        // 新增格子才 Instantiate,已存在的格子只更新颜色
        Color finalColor = valid ? Color.white : invalidColor;

        foreach (Vector2Int grid in reusablePathCells)
        {
            if (previewRootMap.TryGetValue(grid, out Building existing) && existing != null)
            {
                SetColor(existing.gameObject, finalColor);
                continue;
            }

            Building root = Object.Instantiate(
                prefab,
                map.GridToWorld(grid),
                Quaternion.identity
            );

            root.name = $"Root_Preview_{grid.x}_{grid.y}";

            DisablePreviewComponents(root.gameObject);
            SetPreviewSortingOrder(root.gameObject);
            SetColor(root.gameObject, finalColor);

            previewRootMap[grid] = root;
        }
    }




    // [修改] 改为填充传入的集合,避免每帧 new HashSet
    private void GetTargetCells(
        Vector2Int center,
        HashSet<Vector2Int> result)
    {
        if (buildingData == null)
            return;

        Vector2Int[] cells =
            buildingData.GetOccupiedCells();

        if (cells == null)
            return;

        foreach (Vector2Int offset in cells)
        {
            result.Add(center + offset);
        }
    }




    public void Clear()
    {
        if (previewBuilding != null)
        {
            Object.Destroy(
                previewBuilding.gameObject
            );

            previewBuilding = null;
        }



        ClearRootPreview();
    }




    // [修改] 清空字典而非 List
    private void ClearRootPreview()
    {
        foreach (KeyValuePair<Vector2Int, Building> pair in previewRootMap)
        {
            if (pair.Value != null)
            {
                Object.Destroy(pair.Value.gameObject);
            }
        }

        previewRootMap.Clear();
    }




    private void DisablePreviewComponents(
        GameObject target)
    {
        if (target == null)
            return;



        Collider2D[] colliders =
            target.GetComponentsInChildren<Collider2D>(
                true
            );



        foreach (Collider2D collider in colliders)
        {
            collider.enabled = false;
        }




        MonoBehaviour[] behaviours =
            target.GetComponentsInChildren<MonoBehaviour>(
                true
            );



        foreach (MonoBehaviour behaviour in behaviours)
        {
            behaviour.enabled = false;
        }
    }




    private void SetPreviewSortingOrder(
        GameObject target)
    {
        SpriteRenderer[] sprites =
            target.GetComponentsInChildren<SpriteRenderer>(
                true
            );



        foreach (SpriteRenderer sprite in sprites)
        {
            sprite.sortingOrder =
                previewSortingOrder;
        }
    }




    // [修改] 非 SpriteRenderer 分支改用 MaterialPropertyBlock,避免 renderer.material 每次克隆材质实例
    private static readonly MaterialPropertyBlock propertyBlock =
        new MaterialPropertyBlock();

    private void SetColor(
        GameObject target,
        Color color)
    {
        SpriteRenderer[] sprites =
            target.GetComponentsInChildren<SpriteRenderer>(
                true
            );



        Color final =
            color;


        final.a =
            previewAlpha;




        foreach (SpriteRenderer sprite in sprites)
        {
            sprite.color =
                final;
        }





        Renderer[] renderers =
            target.GetComponentsInChildren<Renderer>(
                true
            );



        foreach (Renderer renderer in renderers)
        {
            if (renderer is SpriteRenderer)
                continue;

            if (!renderer.sharedMaterial ||
                !renderer.sharedMaterial.HasProperty("_Color"))
                continue;

            propertyBlock.Clear();
            renderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor("_Color", final);
            renderer.SetPropertyBlock(propertyBlock);
        }
    }
}