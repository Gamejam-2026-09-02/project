using UnityEngine;

[CreateAssetMenu(
    fileName = "UnitData",
    menuName = "Game/Unit Data"
)]
public class UnitData : ScriptableObject
{
    public string unitName;


    [Header("属性")]
    public int maxHealth = 100;

    public int attackDamage = 10;

    public float attackInterval = 1f;

    public float attackRange = 1.5f;



    [Header("移动")]
    public float moveSpeed = 3f;



    [Header("攻击目标")]
    public BuildingType targetBuildingType =
        BuildingType.Home;
}