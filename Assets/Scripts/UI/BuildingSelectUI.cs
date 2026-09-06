using UnityEngine;
using UnityEngine.UI;


public class BuildingSelectUI : MonoBehaviour
{
    [SerializeField]
    private Transform content;


    [SerializeField]
    private BuildingSelectButton buttonPrefab;



    [SerializeField]
    private BuildingPlacementController placementController;



    private void OnEnable()
    {
        if (BuildingUnlockManager.Instance == null)
            return;


        BuildingUnlockManager.Instance
            .OnBuildingListChanged += Refresh;


        Refresh();
    }



    private void OnDisable()
    {
        if (BuildingUnlockManager.Instance == null)
            return;


        BuildingUnlockManager.Instance
            .OnBuildingListChanged -= Refresh;
    }



    private void Refresh()
    {
        Clear();


        foreach (BuildingData data in
            BuildingUnlockManager.Instance.UnlockedBuildings)
        {
            BuildingSelectButton button =
                Instantiate(
                    buttonPrefab,
                    content
                );


            button.Initialize(
                data,
                placementController
            );
        }
    }



    private void Clear()
    {
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            Destroy(
                content.GetChild(i).gameObject
            );
        }
    }
}