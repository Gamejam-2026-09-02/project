using System.Collections.Generic;
using UnityEngine;


public class BuildingPlacementController : MonoBehaviour
{
    private enum PreviewState
    {
        None,
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

    private BuildingPlacementValidator validator;

    private BuildingPlacementExecutor executor;



    public static Dictionary<ResourceType, int> CurrentCost
    {
        get;
        private set;
    }
    = new();



    private void Awake()
    {
        pathFinder =
            new BuildingPlacementPath();


        preview =
            new BuildingPlacementPreview(
                buildingData,
                rootBuildingData
            );


        validator =
            new BuildingPlacementValidator(
                rootBuildingData,
                pathFinder
            );


        executor =
            new BuildingPlacementExecutor(
                rootBuildingData,
                pathFinder
            );
    }



    private void Start()
    {
        state =
            PreviewState.None;

        preview.Clear();

        CurrentCost.Clear();
    }



    private void Update()
    {
        if (GameStateController.Instance.IsPaused)
            return;


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



    /// <summary>
    /// 外部调用入口
    /// </summary>
    public void SetBuildingData(
        BuildingData data)
    {
        if (data == null)
            return;


        buildingData = data;


        preview.SetBuildingData(
            data
        );


        EnterSelectStart();
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


        if (map == null || cam == null)
            return;



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

        CurrentCost.Clear();

        preview.Clear();
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
            ExitPlacement();
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


        if (map == null || cam == null)
            return;



        Vector3 world =
            cam.ScreenToWorldPoint(
                Input.mousePosition
            );


        previewGridPosition =
            map.WorldToGrid(world);



        preview.SetPosition(
            previewGridPosition
        );



        bool changed =
            UpdateConnectionPreviewState(
                previewGridPosition
            );



        if (hasLastPreviewPosition &&
           lastPreviewGridPosition == previewGridPosition &&
           !changed)
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


            preview.SetBuildingPreviewVisible(false);
        }
        else
        {
            preview.SetBuildingPreviewVisible(true);
        }



        return oldTarget != previewTargetBuilding;
    }



    private void RecalculatePreview()
    {
        PlacementResult result =
            validator.Validate(
                startBuilding,
                startGridPosition,
                previewGridPosition,
                previewTargetBuilding,
                buildingData
            );



        currentPath =
            result.path;


        isPreviewValid =
            result.valid;



        preview.SetValid(
            isPreviewValid
        );



        Vector2Int target =
            previewTargetBuilding != null ?
            previewTargetBuilding.GridPosition :
            previewGridPosition;



        preview.ShowRoots(
            currentPath,
            target,
            isPreviewValid
        );


        UpdateCurrentCost();
    }



    private void UpdateCurrentCost()
    {
        CurrentCost.Clear();


        if (currentPath == null)
            return;


        AddCost(
            buildingData.Costs
        );


        for (int i = 0; i < currentPath.Count; i++)
        {
            AddCost(
                rootBuildingData.Costs
            );
        }
    }



    private void AddCost(
        ResourceCost[] costs)
    {
        if (costs == null)
            return;


        foreach (ResourceCost cost in costs)
        {
            if (!CurrentCost.ContainsKey(cost.type))
            {
                CurrentCost[cost.type] = 0;
            }


            CurrentCost[cost.type] += cost.amount;
        }
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
        if (executor.Connect(
            startBuilding,
            target))
        {
            ExitPlacement();
        }
    }



    private void TryGenerate()
    {
        if (!isPreviewValid)
            return;


        if (!executor.Generate(
            currentPath,
            previewGridPosition,
            buildingData))
        {
            return;
        }


        ExitPlacement();
    }



    private void ExitPlacement()
    {
        state =
            PreviewState.None;


        hasStart = false;

        startBuilding = null;

        previewTargetBuilding = null;

        currentPath = null;

        isPreviewValid = false;

        hasLastPreviewPosition = false;


        CurrentCost.Clear();


        preview.Clear();


        if (BuildingDataPanelController.Instance != null)
        {
            BuildingDataPanelController.Instance.Hide();
        }
    }



    private void OnDestroy()
    {
        preview.Clear();
    }
}