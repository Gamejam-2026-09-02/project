using TMPro;
using UnityEngine;


public class BuildingInfoPanelController : MonoBehaviour
{
    [Header("Ãæ°å")]
    [SerializeField]
    private GameObject panel;


    [Header("ÐÅÏ¢ÎÄ±¾")]
    [SerializeField]
    private TMP_Text infoText;


    [Header("¹¥»÷·¶Î§")]
    [SerializeField]
    private AttackRangeCircle rangeCircle;



    private Building currentBuilding;



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


        if (currentBuilding != null)
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
        currentBuilding = building;

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
        BuildingData data =
            currentBuilding.Data;


        infoText.text =
            $"Ãû³Æ: {data.Name}\n\n" +
            $"{data.Description}\n\n" +
            $"ÉúÃü: {currentBuilding.CurrentHealth}/{currentBuilding.MaxHealth}\n" +
            $"¹¥»÷: {data.AttackDamage}\n" +
            $"¹¥»÷¼ä¸ô: {data.AttackInterval:F2}s\n" +
            $"¹¥»÷·¶Î§: {data.AttackRange}";
    }



    private void Close()
    {
        currentBuilding = null;

        panel.SetActive(false);


        if (rangeCircle != null)
            rangeCircle.Hide();
    }
}