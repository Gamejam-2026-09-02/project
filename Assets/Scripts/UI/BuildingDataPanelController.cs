using System.Text;
using TMPro;
using UnityEngine;


public class BuildingDataPanelController : MonoBehaviour
{
    public static BuildingDataPanelController Instance { get; private set; }


    [Header("面板")]
    [SerializeField]
    private GameObject panel;


    [Header("信息文本")]
    [SerializeField]
    private TMP_Text infoText;



    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }



    private void Start()
    {
        Hide();
    }



    public void Show(BuildingData data)
    {
        if (data == null)
            return;


        StringBuilder builder = new();


        builder.AppendLine(
            $"{data.Name}"
        );

        builder.AppendLine();


        builder.AppendLine(
            data.Description
        );

        builder.AppendLine();


        builder.AppendLine(
            $"生命: {data.MaxHP}"
        );


        builder.AppendLine(
            $"攻击: {data.AttackDamage}"
        );


        builder.AppendLine(
            $"攻击间隔: {data.AttackInterval:F2}s"
        );


        builder.AppendLine(
            $"攻击范围: {data.AttackRange}"
        );


        // 每秒资源变化
        if (data.resourceChange.amount != 0)
        {
            string type =
                data.resourceChange.type.ToString();


            if (data.resourceChange.amount > 0)
            {
                builder.AppendLine(
                    $"每秒获取: {type} +{data.resourceChange.amount}"
                );
            }
            else
            {
                builder.AppendLine(
                    $"每秒消耗: {type} {data.resourceChange.amount}"
                );
            }
        }


        // 建造消耗
        if (data.Costs != null &&
           data.Costs.Length > 0)
        {
            builder.AppendLine();

            builder.AppendLine(
                "建造消耗:"
            );


            foreach (ResourceCost cost in data.Costs)
            {
                builder.AppendLine(
                    $"{cost.type}: {cost.amount}"
                );
            }
        }


        infoText.text =
            builder.ToString();


        panel.SetActive(true);
    }



    public void Hide()
    {
        panel.SetActive(false);
    }
}