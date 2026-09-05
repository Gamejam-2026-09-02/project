using UnityEngine;

public class EnemySpawnTest : MonoBehaviour
{
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
        if (unitData == null)
        {
            Debug.LogError("UnitData 未设置");
            return;
        }


        if (unitData.prefab == null)
        {
            Debug.LogError("UnitData 中的 unitPrefab 未设置");
            return;
        }


        GameObject unit = Instantiate(
            unitData.prefab,
            transform.position,
            Quaternion.identity
        );


        UnitMover mover = unit.GetComponent<UnitMover>();

        if (mover != null)
        {
            mover.Initialize(unitData);
        }


        UnitHealth health = unit.GetComponent<UnitHealth>();

        if (health != null)
        {
            health.Initialize(unitData);
        }
    }
}