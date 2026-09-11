using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class BuildingResourceUI : MonoBehaviour
{
    [SerializeField]
    private Transform content;


    [SerializeField]
    private TextMeshProUGUI textPrefab;



    private Dictionary<ResourceType, TextMeshProUGUI> rows =
        new();

    // 新增：复用的字典，避免每帧 Update 中 new Dictionary 产生 GC
    private readonly Dictionary<ResourceType, int> totalChangeCache =
        new();



    private void Update()
    {
        Refresh();
    }



    private void Refresh()
    {
        if (PlayerGameDataManager.Instance == null)
            return;


        Dictionary<ResourceType, int> cost =
            BuildingPlacementController.CurrentCost;

        // 新增：汇总全局建筑资源变化量
        Dictionary<ResourceType, int> totalChange =
            GetTotalResourceChange();



        foreach (ResourceType type in
                System.Enum.GetValues(typeof(ResourceType)))
        {
            int current =
                PlayerGameDataManager.Instance
                .GetResource(type);



            int need = 0;


            if (cost != null &&
               cost.TryGetValue(type, out int value))
            {
                need = value;
            }

            // 新增：取出该资源的全局变化量
            totalChange.TryGetValue(type, out int change);



            if (current <= 0 && need <= 0 && change == 0)
                continue;



            if (!rows.TryGetValue(
                type,
                out TextMeshProUGUI text))
            {
                text =
                    Instantiate(
                        textPrefab,
                        content
                    );
                Debug.Log(
    $"创建资源UI {type}  来自 {gameObject.name}"
);


                rows.Add(
                    type,
                    text
                );
            }

            // 新增：变化量文本，正数带 + 号，0 则不显示
            string changeText =
                change != 0
                    ? $" ({(change > 0 ? "+" : "")}{change})"
                    : "";



            if (need > 0)
            {
                bool enough =
                    current >= need;


                text.text =
                    $"{type}  {current}{changeText}  " +
                    $"<color=#{(enough ? "FFFFFF" : "FF0000")}>-{need}</color>";
            }
            else
            {
                text.text =
                    $"{type}  {current}{changeText}";
            }
        }
    }


    // 新增：遍历 BuildingManager 中所有建筑，汇总 resourceChange
    private Dictionary<ResourceType, int> GetTotalResourceChange()
    {
        totalChangeCache.Clear();

        if (BuildingManager.Instance == null)
            return totalChangeCache;

        IReadOnlyList<Building> buildings =
            BuildingManager.Instance.Buildings;

        for (int i = 0; i < buildings.Count; i++)
        {
            Building building = buildings[i];

            if (building == null)
                continue;

            ResourceCost change =
                building.Data.resourceChange;

            totalChangeCache.TryGetValue(
                change.type,
                out int existing);

            totalChangeCache[change.type] =
                existing + change.amount;
        }

        return totalChangeCache;
    }
}