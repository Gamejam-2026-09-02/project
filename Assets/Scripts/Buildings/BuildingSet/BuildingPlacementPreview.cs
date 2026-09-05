using System.Collections.Generic;
using UnityEngine;


public class BuildingPlacementPreview
{
    private readonly BuildingData buildingData;
    private readonly BuildingData rootBuildingData;


    private readonly float previewAlpha = 0.5f;
    private readonly Color invalidColor = Color.red;
    private readonly int previewSortingOrder = 100;


    private Building previewBuilding;


    private readonly List<Building> previewRoots =
        new List<Building>();



    public BuildingPlacementPreview(
        BuildingData buildingData,
        BuildingData rootBuildingData)
    {
        this.buildingData = buildingData;
        this.rootBuildingData = rootBuildingData;
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




    /// <summary>
    /// 恢复建筑预览默认颜色
    /// 用于连接模式
    /// </summary>
    public void ResetColor()
    {
        if (previewBuilding == null)
            return;


        SetColor(
            previewBuilding.gameObject,
            Color.white
        );
    }




    /// <summary>
    /// 控制建筑Ghost显示
    /// 连接已有建筑时隐藏
    /// </summary>
    public void SetBuildingPreviewVisible(
        bool value)
    {
        if (previewBuilding == null)
            return;


        previewBuilding.gameObject.SetActive(
            value
        );
    }




    public void ShowRoots(
        List<Vector2> path,
        Vector2Int targetPosition,
        bool valid)
    {
        ClearRootPreview();


        if (path == null ||
            path.Count == 0 ||
            rootBuildingData == null)
        {
            return;
        }



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
            return;




        HashSet<Vector2Int> targetCells =
            GetTargetCells(
                targetPosition
            );


        HashSet<Vector2Int> generated =
            new HashSet<Vector2Int>();




        foreach (Vector2 point in path)
        {
            Vector2Int grid =
                map.WorldToGrid(point);



            if (targetCells.Contains(grid))
                continue;



            if (!generated.Add(grid))
                continue;



            Building root =
                Object.Instantiate(
                    prefab,
                    map.GridToWorld(grid),
                    Quaternion.identity
                );



            root.name =
                $"Root_Preview_{grid.x}_{grid.y}";



            DisablePreviewComponents(
                root.gameObject
            );


            SetPreviewSortingOrder(
                root.gameObject
            );



            SetColor(
                root.gameObject,
                valid
                    ? Color.white
                    : invalidColor
            );



            previewRoots.Add(root);
        }
    }




    private HashSet<Vector2Int> GetTargetCells(
        Vector2Int center)
    {
        HashSet<Vector2Int> result =
            new HashSet<Vector2Int>();


        if (buildingData == null)
            return result;



        Vector2Int[] cells =
            buildingData.GetOccupiedCells();



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




    private void ClearRootPreview()
    {
        for (int i = 0; i < previewRoots.Count; i++)
        {
            if (previewRoots[i] != null)
            {
                Object.Destroy(
                    previewRoots[i].gameObject
                );
            }
        }



        previewRoots.Clear();
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



            Material material =
                renderer.material;



            if (material != null &&
                material.HasProperty("_Color"))
            {
                material.color =
                    final;
            }
        }
    }
}