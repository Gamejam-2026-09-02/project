using System;
using UnityEngine;

[Serializable]
public class PlayerGameData
{
    [Header("资源")]
    public int Water;
    public int Wood;
    public int Stone;

    [Header("主城")]
    public int MainCityHealth;
    public int MainCityMaxHealth;

    [Header("怪物波次")]
    public int CurrentWave;
}