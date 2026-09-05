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


        Building target =
            mover.TargetBuilding;

        if (target == null)
            return;

        float distance =
      Vector2.Distance(
          transform.position,
          target.transform.position
      )
      - Mathf.Max(
          target.transform.localScale.x,
          target.transform.localScale.y
      ) * 0.5f;

        if (distance > data.attackRange)
            return;



        attackTimer -= Time.deltaTime;


        if (attackTimer <= 0)
        {
            Attack(target);

            attackTimer = data.attackInterval;
        }
    }



    private void Attack(Building target)
    {
        Debug.Log(
            $"{name} ¹¥»÷ {target.name}£¬Ôì³É {data.attackDamage} µãÉËº¦¡£",
            this
        );
        target.TakeDamage(
            data.attackDamage
        );
    }
}