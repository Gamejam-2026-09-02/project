using UnityEngine;

public class UnitCombat : MonoBehaviour
{
    private UnitMover mover;

    private UnitData data;

    private float attackTimer;


    private void Awake()
    {
        mover = GetComponent<UnitMover>();
    }


    private void Start()
    {
        data = mover.data;
    }


    private void Update()
    {
        if (data == null)
            return;


        Building target = mover.TargetBuilding;

        if (target == null)
            return;


        if (!InAttackRange(target))
            return;


        attackTimer -= Time.deltaTime;


        if (attackTimer <= 0)
        {
            Attack(target);

            attackTimer = data.attackInterval;
        }
    }



    private bool InAttackRange(Building target)
    {
        Collider2D targetCollider =
            target.GetComponent<Collider2D>();

        if (targetCollider == null)
            return false;


        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                transform.position,
                data.attackRange
            );


        foreach (Collider2D hit in hits)
        {
            if (hit == targetCollider)
                return true;
        }


        return false;
    }



    private void Attack(Building target)
    {
        target.TakeDamage(
            data.attackDamage
        );
    }



    private void OnDrawGizmosSelected()
    {
        if (data == null)
            return;


        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            data.attackRange
        );
    }
}