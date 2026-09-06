using System.Collections.Generic;
using UnityEngine;


public class BuildingPlacementController : MonoBehaviour
{
    private enum PreviewState
    {
        SelectStart,
        SelectTarget
    }



    [Header("Building")]
    [SerializeField]
    private BuildingData buildingData;



    [Header("Root")]
    [SerializeField]
    private BuildingData rootBuildingData;



    private PreviewState state;



    private Building startBuilding;

    private Vector2Int startGridPosition;



    private Vector2Int previewGridPosition;



    private Vector2Int lastPreviewGridPosition;

    private bool hasLastPreviewPosition;



    private bool hasStart;

    private bool isPreviewValid;



    private List<Vector2> currentPath;



    private Building previewTargetBuilding;



    private BuildingPlacementPreview preview;

    private BuildingPlacementPath pathFinder;

    private BuildingConnector connector;




    private void Awake()
    {
        preview =
            new BuildingPlacementPreview(
                buildingData,
                rootBuildingData
            );


        pathFinder =
            new BuildingPlacementPath();



        connector =
            new BuildingConnector(
                rootBuildingData,
                pathFinder
            );
    }




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


        Camera cam =
            Camera.main;


        if (map == null ||
           cam == null)
        {
            return;
        }



        Vector3 world =
            cam.ScreenToWorldPoint(
                Input.mousePosition
            );



        Vector2Int grid =
            map.WorldToGrid(world);



        GridNode node =
            map.GetNode(grid);



        if (node == null ||
           !node.IsOccupied)
        {
            return;
        }



        startBuilding =
            node.Occupant;



        if (startBuilding == null)
            return;



        startGridPosition =
            grid;


        hasStart = true;


        EnterSelectTarget();
    }






    private void EnterSelectTarget()
    {
        state =
            PreviewState.SelectTarget;


        preview.Create();


        hasLastPreviewPosition = false;
    }






    private void UpdateSelectTarget()
    {
        UpdatePreview();



        if (Input.GetMouseButtonDown(0))
        {
            TryGenerateOrConnect();
        }



        if (Input.GetMouseButtonDown(1) ||
           Input.GetKeyDown(KeyCode.Escape))
        {
            EnterSelectStart();
        }
    }







    private void UpdatePreview()
    {
        if (!hasStart)
            return;



        GridMapManager map =
            GridMapManager.Instance;


        Camera cam =
            Camera.main;



        if (map == null ||
           cam == null)
        {
            return;
        }



        Vector3 world =
            cam.ScreenToWorldPoint(
                Input.mousePosition
            );



        previewGridPosition =
            map.WorldToGrid(world);



        preview.SetPosition(
            previewGridPosition
        );



        // 每次检测鼠标当前建筑状态
        bool connectionStateChanged =
            UpdateConnectionPreviewState(
                previewGridPosition
            );



        // 格子没有变化并且目标状态没有变化
        // 不重新寻路
        if (hasLastPreviewPosition &&
           lastPreviewGridPosition ==
           previewGridPosition &&
           !connectionStateChanged)
        {
            return;
        }



        lastPreviewGridPosition =
            previewGridPosition;


        hasLastPreviewPosition = true;



        RecalculatePreview();
    }







    private bool UpdateConnectionPreviewState(
        Vector2Int grid)
    {
        Building oldTarget =
            previewTargetBuilding;



        previewTargetBuilding = null;



        GridNode node =
            GridMapManager.Instance
            .GetNode(grid);



        if (node != null &&
           node.IsOccupied &&
           node.Occupant != null &&
           node.Occupant != startBuilding &&
           !node.Occupant.ConnectedToHome)
        {
            previewTargetBuilding =
                node.Occupant;



            preview.SetBuildingPreviewVisible(
                false
            );
        }
        else
        {
            preview.SetBuildingPreviewVisible(
                true
            );
        }



        return oldTarget != previewTargetBuilding;
    }








    private void RecalculatePreview()
    {
        // ==========================
        // 连接模式
        // ==========================

        if (previewTargetBuilding != null)
        {
            currentPath =
                pathFinder.FindPath(
                    startBuilding,
                    startGridPosition,
                    previewTargetBuilding.GridPosition,
                    previewTargetBuilding.Data
                );



            bool valid =
                currentPath != null &&
                currentPath.Count > 0 &&
                pathFinder.CanGenerateRoots(
                    currentPath,
                    previewTargetBuilding.GridPosition,
                    rootBuildingData
                );



            isPreviewValid =
                valid;



            // 清除之前红色状态
            preview.ResetColor();



            preview.ShowRoots(
                currentPath,
                previewTargetBuilding.GridPosition,
                valid
            );


            return;
        }






        // ==========================
        // 建造模式
        // ==========================


        currentPath =
            pathFinder.FindPath(
                startBuilding,
                startGridPosition,
                previewGridPosition,
                buildingData
            );



        bool hasPath =
            currentPath != null &&
            currentPath.Count > 0;



        bool canBuilding =
            BuildingGenerator.Instance != null &&
            BuildingGenerator.Instance.CanGenerate(
                buildingData,
                previewGridPosition
            );



        bool canRoot =
            hasPath &&
            pathFinder.CanGenerateRoots(
                currentPath,
                previewGridPosition,
                rootBuildingData
            );



        isPreviewValid =
            hasPath &&
            canBuilding &&
            canRoot;



        preview.SetValid(
            isPreviewValid
        );



        preview.ShowRoots(
            currentPath,
            previewGridPosition,
            isPreviewValid
        );
    }








    private void TryGenerateOrConnect()
    {
        if (previewTargetBuilding != null)
        {
            TryConnect(
                previewTargetBuilding
            );


            return;
        }



        TryGenerate();
    }







    private void TryConnect(
        Building target)
    {
        if (connector.TryConnect(
            startBuilding,
            target))
        {
            EnterSelectStart();
        }
    }







    private void TryGenerate()
    {
        if (!isPreviewValid)
            return;



        if (!pathFinder.GenerateRoots(
            currentPath,
            previewGridPosition,
            rootBuildingData))
        {
            return;
        }



        BuildingGenerator generator =
            BuildingGenerator.Instance;



        if (generator == null)
            return;



        Building building =
            generator.Generate(
                buildingData,
                previewGridPosition
            );



        if (building == null)
            return;



        EnterSelectStart();
    }







    private void EnterSelectStart()
    {
        state =
            PreviewState.SelectStart;



        hasStart = false;


        startBuilding = null;


        previewTargetBuilding = null;


        currentPath = null;


        isPreviewValid = false;


        hasLastPreviewPosition = false;



        preview.Clear();
    }







    private void OnDestroy()
    {
        preview.Clear();
    }
}