using UnityEngine;

public class BuildingGeneratorTest : MonoBehaviour
{
    [SerializeField] private BuildingData buildingData;


    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryGenerate();
        }
    }


    private void TryGenerate()
    {
        if (buildingData == null)
        {
            Debug.LogError("没有配置 BuildingData。");
            return;
        }

        Camera camera = Camera.main;

        if (camera == null)
        {
            Debug.LogError("没有找到 Main Camera。");
            return;
        }

        Vector3 worldPosition =
            camera.ScreenToWorldPoint(
                Input.mousePosition
            );

        Vector2Int gridPosition =
            GridMapManager.Instance.WorldToGrid(
                worldPosition
            );

        Building building =
            BuildingGenerator.Instance.Generate(
                buildingData,
                gridPosition
            );

        if (building != null)
        {
            Debug.Log(
                $"建筑生成成功：{buildingData.Name}，位置：{gridPosition}"
            );
        }
        else
        {
            Debug.Log(
                $"建筑生成失败：{gridPosition}"
            );
        }
    }
}