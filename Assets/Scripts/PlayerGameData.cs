using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerGameData
{
    [Header("资源")]

    public Dictionary<ResourceType, int> Resources =
        new();

    [Header("主城")]
    public int MainCityHealth;
    public int MainCityMaxHealth;

    [Header("怪物波次")]
    public int CurrentWave;
}