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



            if (current <= 0 && need <= 0)
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



            if (need > 0)
            {
                bool enough =
                    current >= need;


                text.text =
                    $"{type}  {current}  " +
                    $"<color=#{(enough ? "FFFFFF" : "FF0000")}>-{need}</color>";
            }
            else
            {
                text.text =
                    $"{type}  {current}";
            }
        }
    }
}