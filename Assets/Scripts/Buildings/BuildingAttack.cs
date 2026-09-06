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



        UnitMover target =
            RTSUnitManager.Instance
            .FindNearest(transform.position);



        if (target == null)
            return;



        float distance =
            Vector2.Distance(
                transform.position,
                target.transform.position
            );


        if (distance > data.AttackRange)
            return;



        timer -= Time.deltaTime;


        if (timer <= 0)
        {
            Attack(target);

            timer = data.AttackInterval;
        }
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