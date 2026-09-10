using UnityEngine;


public class BuildingAttack : MonoBehaviour
{
    private Building building;

    private float timer;



    private void Awake()
    {
        building = GetComponent<Building>();
    }



    private void Update()
    {
        if (building == null)
            return;

        if (!building.ConnectedToHome)
            return;

        BuildingData data = building.Data;

        if (data == null)
            return;

        timer -= Time.deltaTime;

        if (timer > 0)
            return;

        UnitMover target =
            RTSUnitManager.Instance.FindNearest(
                transform.position,
                data.AttackRange
            );

        if (target == null)
            return;

        Attack(target);

        timer = data.AttackInterval;
    }


    private void Attack(UnitMover target)
    {
        UnitCore health =
            target.GetComponent<UnitCore>();


        if (health != null)
        {
            health.TakeDamage(
                building.Data.AttackDamage
            );
        }
    }
}