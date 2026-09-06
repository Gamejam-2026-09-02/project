using UnityEngine;
using UnityEngine.UI;


public class BuildingSelectButton : MonoBehaviour
{
    private BuildingData buildingData;


    private BuildingPlacementController controller;


    private Button button;



    public void Initialize(
        BuildingData data,
        BuildingPlacementController controller)
    {
        buildingData = data;
        this.controller = controller;


        button =
            GetComponent<Button>();


        button.onClick.AddListener(
            Select
        );
    }



    private void Select()
    {
        controller.SetBuildingData(
            buildingData
        );


        Debug.Log(
            $"Ñ¡Ôñ½¨Öþ: {buildingData.name}"
        );
    }



    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(
                Select
            );
        }
    }
}