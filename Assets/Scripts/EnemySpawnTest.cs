using UnityEngine;

public class EnemySpawnTest : MonoBehaviour
{
    [SerializeField]
    private GameObject unitPrefab;

    [SerializeField]
    private UnitData unitData;


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Spawn();
        }
    }


    private void Spawn()
    {
        if (unitPrefab == null)
        {
            Debug.LogError("Unit prefab Œ¥…Ë÷√");
            return;
        }

        if (unitData == null)
        {
            Debug.LogError("UnitData Œ¥…Ë÷√");
            return;
        }


        GameObject unit =
            Instantiate(
                unitPrefab,
                transform.position,
                Quaternion.identity
            );


        UnitMover mover =
            unit.GetComponent<UnitMover>();

        if (mover != null)
        {
            mover.Initialize(unitData);
        }


        UnitHealth health =
            unit.GetComponent<UnitHealth>();

        if (health != null)
        {
            health.Initialize(unitData);
        }
    }
}