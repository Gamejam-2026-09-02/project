using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class EnemySpawner : MonoBehaviour
{
    [Serializable]
    public class EnemyEntry
    {
        public UnitData unitData;

        [Min(0)]
        public int weight = 1;
    }


    [Serializable]
    public class WaveConfig
    {
        [Min(1)]
        public int totalEnemyCount = 10;
    }


    [Header("生成点")]
    [SerializeField]
    private Transform[] spawnPoints;


    [Header("敌人列表")]
    [SerializeField]
    private List<EnemyEntry> enemies = new();


    [Header("生成点选择")]
    [SerializeField]
    private int spawnPointCount = 3;


    [SerializeField]
    private float minSpawnPointDistance = 5f;



    [Header("波次配置")]
    [SerializeField]
    private List<WaveConfig> waves = new();


    [SerializeField]
    private float waveInterval = 30f;


    [SerializeField]
    private TMP_Text waveText;



    [Header("敌人位置")]
    [SerializeField]
    private float spawnRadius = 3f;


    [SerializeField]
    private float minEnemyDistance = 1f;



    private readonly List<Vector3> spawnedPositions = new();


    private int currentWaveIndex = -1;

    private float waveTimer = 0f;

    private bool allWavesFinished = false;



    private void Start()
    {
        UpdateWaveText();

        Invoke("RestTime",1f);
    }

    public void RestTime()
    {
        waveTimer = waveInterval;
    }

    private void Update()
    {
        if (allWavesFinished)
            return;


        waveTimer += Time.deltaTime;


        if (waveTimer >= waveInterval)
        {
            waveTimer = 0f;
            SpawnNextWave();
        }


        //if (Input.GetKeyDown(spawnKey))
        //{
        //    waveTimer = 0f;
        //    SpawnNextWave();
        //}
    }



    public void SpawnNextWave()
    {
        if (allWavesFinished)
            return;


        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("没有设置生成点");
            return;
        }


        currentWaveIndex++;


        if (currentWaveIndex >= waves.Count)
        {
            allWavesFinished = true;
            currentWaveIndex = waves.Count - 1;
            UpdateWaveText();
            return;
        }


        spawnedPositions.Clear();


        List<Transform> selectedPoints = GetRandomSpawnPoints();


        SpawnEnemiesAtPoints(
            selectedPoints,
            waves[currentWaveIndex].totalEnemyCount
        );


        UpdateWaveText();


        if (currentWaveIndex >= waves.Count - 1)
        {
            allWavesFinished = true;
        }
    }



    /// <summary>
    /// 将本波总数量均分到各生成点，余数依次分配给前几个点
    /// </summary>
    private void SpawnEnemiesAtPoints(
        List<Transform> points,
        int totalCount
    )
    {
        if (points.Count == 0)
            return;


        int baseCount = totalCount / points.Count;
        int remainder = totalCount % points.Count;


        for (int i = 0; i < points.Count; i++)
        {
            int count = baseCount + (i < remainder ? 1 : 0);

            Transform point = points[i];

            EnemySpawnAlertManager.Instance?.ShowAlert(point.position);


            for (int j = 0; j < count; j++)
            {
                Vector3 position = GetValidSpawnPosition(
                    point.position
                );

                SpawnEnemy(position);
            }
        }
    }



    private void UpdateWaveText()
    {
        if (waveText == null)
            return;


        int displayWave = Mathf.Clamp(
            currentWaveIndex + 1,
            0,
            waves.Count
        );


        waveText.text = $"{displayWave}/{waves.Count}";
    }



    /// <summary>
    /// 随机选择尽可能不相邻的生成点
    /// </summary>
    private List<Transform> GetRandomSpawnPoints()
    {
        List<Transform> available = new(spawnPoints);

        List<Transform> result = new();


        int targetCount = Mathf.Min(
            spawnPointCount,
            available.Count
        );


        int tryCount = 0;


        while (result.Count < targetCount && available.Count > 0)
        {
            int index = UnityEngine.Random.Range(
                0,
                available.Count
            );


            Transform candidate = available[index];

            available.RemoveAt(index);


            bool valid = true;


            foreach (Transform selected in result)
            {
                if (Vector3.Distance(
                    candidate.position,
                    selected.position
                ) < minSpawnPointDistance)
                {
                    valid = false;
                    break;
                }
            }


            if (valid)
            {
                result.Add(candidate);
            }


            tryCount++;


            // 找不到足够分散的位置时取消限制
            if (tryCount > spawnPoints.Length * 3)
            {
                while (
                    result.Count < targetCount &&
                    available.Count > 0
                )
                {
                    int random = UnityEngine.Random.Range(
                        0,
                        available.Count
                    );


                    result.Add(
                        available[random]
                    );


                    available.RemoveAt(random);
                }

                break;
            }
        }


        return result;
    }



    /// <summary>
    /// 获取一个不会和已有敌人重叠的位置
    /// </summary>
    private Vector3 GetValidSpawnPosition(Vector3 center)
    {
        const int maxTry = 30;


        for (int i = 0; i < maxTry; i++)
        {
            Vector2 offset =
                UnityEngine.Random.insideUnitCircle *
                spawnRadius;


            Vector3 position = center + new Vector3(
                offset.x,
                offset.y,
                0
            );


            bool valid = true;


            foreach (Vector3 oldPosition in spawnedPositions)
            {
                if (Vector3.Distance(
                    position,
                    oldPosition
                ) < minEnemyDistance)
                {
                    valid = false;
                    break;
                }
            }


            if (valid)
            {
                spawnedPositions.Add(position);
                return position;
            }
        }


        // 极端情况无法找到空位
        Vector2 fallback =
            UnityEngine.Random.insideUnitCircle *
            spawnRadius;


        Vector3 result = center + new Vector3(
            fallback.x,
            fallback.y,
            0
        );


        spawnedPositions.Add(result);

        return result;
    }



    private void SpawnEnemy(Vector3 position)
    {
        UnitData data = GetRandomEnemy();


        if (data == null)
        {
            Debug.LogError("没有有效敌人配置");
            return;
        }


        if (data.prefab == null)
        {
            Debug.LogError(
                $"敌人 {data.name} 没有Prefab"
            );

            return;
        }


        GameObject unit = Instantiate(
            data.prefab,
            position,
            Quaternion.identity
        );


        UnitCore core = unit.GetComponent<UnitCore>();


        if (core != null)
        {
            core.Initialize(data);
        }
    }



    /// <summary>
    /// 根据权重随机敌人
    /// </summary>
    private UnitData GetRandomEnemy()
    {
        int totalWeight = 0;


        foreach (EnemyEntry entry in enemies)
        {
            if (entry.unitData != null)
            {
                totalWeight += entry.weight;
            }
        }


        if (totalWeight <= 0)
            return null;



        int randomValue = UnityEngine.Random.Range(
            0,
            totalWeight
        );


        foreach (EnemyEntry entry in enemies)
        {
            if (entry.unitData == null)
                continue;


            randomValue -= entry.weight;


            if (randomValue < 0)
            {
                return entry.unitData;
            }
        }


        return null;
    }
}