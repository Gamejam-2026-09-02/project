using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    [SerializeField]
    private BuildingData mainCityData;


    private Building mainCity;


    public GameObject overPlane;

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


        BuildingGenerator generator = BuildingGenerator.Instance;

        if (generator == null)
        {
            Debug.LogError("没有找到 BuildingGenerator。");
            return;
        }


        Vector2Int center = new Vector2Int(
            map.GridSize.x / 2,
            map.GridSize.y / 2
        );


        mainCity = generator.Generate(
            mainCityData,
            center
        );


        if (mainCity == null)
        {
            Debug.LogError("主城生成失败。");
            return;
        }


        mainCity.OnDestroyed += OnMainCityDestroyed;


        BuildingConnectionManager connection =
            BuildingConnectionManager.Instance;


        if (connection != null)
        {
            connection.SetHome(mainCity);
            connection.Refresh();
        }
    }


    private void OnMainCityDestroyed()
    {
        Debug.Log("主城被摧毁，游戏结束");

        GameOver();
    }


    private void GameOver()
    {
        // 结算逻辑
        // 保存数据
        // 显示结算UI
        // 停止游戏
        overPlane.SetActive(true);
        Time.timeScale = 0;
    }


    private void OnDestroy()
    {
        if (mainCity != null)
        {
            mainCity.OnDestroyed -= OnMainCityDestroyed;
        }
    }
}