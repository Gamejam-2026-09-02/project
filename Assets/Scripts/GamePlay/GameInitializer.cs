using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    [SerializeField] private BuildingData mainCityData;

    private void Start()
    {
        GenerateMainCity();
    }

    private void GenerateMainCity()
    {
        if (mainCityData == null)
        {
            Debug.LogError("没有配置主城 BuildingData。");
            return;
        }

        GridMapManager map = GridMapManager.Instance;

        if (map == null)
        {
            Debug.LogError("没有找到 GridMapManager。");
            return;
        }

        BuildingGenerator generator =
            BuildingGenerator.Instance;

        if (generator == null)
        {
            Debug.LogError("没有找到 BuildingGenerator。");
            return;
        }

        Vector2Int center =
            new Vector2Int(
                map.GridSize.x / 2,
                map.GridSize.y / 2
            );

        Building mainCity =
            generator.Generate(
                mainCityData,
                center
            );

        if (mainCity == null)
        {
            Debug.LogError("主城生成失败。");
        }
    }
}