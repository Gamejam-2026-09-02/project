using TMPro;
using UnityEngine;


public class BuildingInfoController : MonoBehaviour
{
    [Header("面板")]
    [SerializeField]
    private GameObject panel;


    [Header("信息文本")]
    [SerializeField]
    private TMP_Text infoText;


    [Header("攻击范围")]
    [SerializeField]
    private AttackRangeCircle rangeCircle;

    // [修改] 删除 currentBuilding 字段，选中状态统一读取 BuildingSelectController.SelectedBuilding，避免重复数据源



    private void Start()
    {
        panel.SetActive(false);
    }



    private void Update()
    {
        if (GameStateController.Instance.IsPaused) return;

        if (Input.GetMouseButtonDown(0))
        {
            TrySelectBuilding();
        }

        // [修改] 从唯一数据源判断是否需要刷新
        if (BuildingSelectController.Instance.SelectedBuilding != null)
        {
            Refresh();
        }
    }



    private void TrySelectBuilding()
    {
        Vector2 mousePosition =
            Camera.main.ScreenToWorldPoint(
                Input.mousePosition
            );


        Collider2D hit =
            Physics2D.OverlapPoint(mousePosition);


        if (hit == null)
        {
            Close();
            return;
        }


        Building building =
            hit.GetComponentInParent<Building>();


        if (building == null)
        {
            Close();
            return;
        }


        Open(building);
    }



    private void Open(Building building)
    {
        // [修改] 先建立选中状态（唯一数据源）与选中框动画，再刷新面板内容
        BuildingSelectController.Instance.SelectBuildingObject(building);

        panel.SetActive(true);

        Refresh();


        if (rangeCircle != null)
        {
            rangeCircle.Show(
                building.transform.position,
                building.Data.AttackRange
            );
        }
    }



    private void Refresh()
    {
        // [修改] 从唯一数据源读取当前选中建筑
        Building building = BuildingSelectController.Instance.SelectedBuilding;

        BuildingData data = building.Data;

        string attackInfo = "";

        if (data.AttackDamage > 0)
        {
            attackInfo =
                $"攻击: {data.AttackDamage}\n" +
                $"攻击间隔: {data.AttackInterval:F2}s\n";
        }

        infoText.text =
            $"{data.Name}\n\n" +
            $"{data.Description}\n\n" +
            $"生命: {building.CurrentHealth}/{building.MaxHealth}\n" +
            attackInfo;
    }

    private void Close()
    {
        panel.SetActive(false);


        if (rangeCircle != null)
            rangeCircle.Hide();

        // [修改] 点击建筑以外区域时同步清除选中状态，使选中框隐藏
        BuildingSelectController.Instance.ClearSelection();
    }
}