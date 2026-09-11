using UnityEngine;


public class BuildingSelectController : MonoBehaviour
{
    public static BuildingSelectController Instance { get; private set; }


    [Header("选中框")]
    [SerializeField]
    private LineRenderer selectionBox;


    [SerializeField]
    private float padding = 0.2f;


    [SerializeField]
    private float lineWidth = 0.08f;


    [SerializeField]
    private Color boxColor = Color.yellow;


    [Header("缩放动画")]
    [SerializeField]
    private float startScale = 1.5f;


    [SerializeField]
    private float animationTime = 0.15f;



    private BuildingData currentBuildingData;


    private Building currentBuilding;


    private Vector3[] boxPoints;


    private float animationTimer;


    private bool playingAnimation;



    public BuildingData CurrentBuildingData =>
        currentBuildingData;

    // [新增] 对外暴露当前选中的建筑实例，作为选中状态的唯一数据源
    public Building SelectedBuilding =>
        currentBuilding;



    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }


        Instance = this;


        if (selectionBox != null)
        {
            selectionBox.loop = true;
            selectionBox.useWorldSpace = true;

            selectionBox.startWidth = lineWidth;
            selectionBox.endWidth = lineWidth;

            selectionBox.startColor = boxColor;
            selectionBox.endColor = boxColor;

            selectionBox.enabled = false;
        }
    }



    private void Update()
    {
        if (!playingAnimation ||
           selectionBox == null)
        {
            return;
        }


        animationTimer += Time.deltaTime;


        float t =
            Mathf.Clamp01(
                animationTimer / animationTime
            );


        float scale =
            Mathf.Lerp(
                startScale,
                1f,
                t
            );


        ApplyScale(scale);


        if (t >= 1f)
        {
            playingAnimation = false;
        }
    }



    public void SelectBuilding(
        BuildingData data)
    {
        if (data == null)
            return;


        currentBuildingData = data;


        BuildingPlacementController placement =
            FindFirstObjectByType<BuildingPlacementController>();


        if (placement != null)
        {
            placement.SetBuildingData(data);
        }


        if (BuildingDataPanelController.Instance != null)
        {
            BuildingDataPanelController.Instance.Show(data);
        }
    }



    public void SelectBuildingObject(
        Building building)
    {
        if (building == null)
            return;


        currentBuilding = building;


        DrawSelectionBox(building);


        animationTimer = 0;

        playingAnimation = true;


        ApplyScale(startScale);
    }



    private void DrawSelectionBox(
        Building building)
    {
        if (selectionBox == null)
            return;


        Vector2Int size =
            building.Data.GetSize();


        float width =
            size.x + padding;


        float height =
            size.y + padding;


        Vector3 center =
            building.transform.position;



        boxPoints = new Vector3[]
        {
            center + new Vector3(-width / 2, -height / 2, 0),
            center + new Vector3(-width / 2,  height / 2, 0),
            center + new Vector3( width / 2,  height / 2, 0),
            center + new Vector3( width / 2, -height / 2, 0)
        };


        selectionBox.positionCount =
            boxPoints.Length;


        selectionBox.SetPositions(
            boxPoints
        );


        selectionBox.enabled = true;
    }



    private void ApplyScale(
        float scale)
    {
        if (boxPoints == null)
            return;


        Vector3 center =
            currentBuilding.transform.position;


        for (int i = 0; i < boxPoints.Length; i++)
        {
            Vector3 offset =
                boxPoints[i] - center;


            selectionBox.SetPosition(
                i,
                center + offset * scale
            );
        }
    }



    public void ClearSelection()
    {
        currentBuildingData = null;

        currentBuilding = null;


        boxPoints = null;


        playingAnimation = false;


        if (selectionBox != null)
        {
            selectionBox.enabled = false;
        }
    }
}