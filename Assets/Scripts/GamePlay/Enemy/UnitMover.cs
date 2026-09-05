using System.Collections.Generic;
using UnityEngine;


public class UnitMover : MonoBehaviour
{
    [Header("单位数据")]
    public UnitData data;


    [Header("单位避让")]
    [SerializeField]
    private float separationRadius = 0.8f;


    [SerializeField]
    private float separationStrength = 2f;


    [Header("转向")]
    [SerializeField]
    private float rotationSpeed = 720f;


    [SerializeField]
    private float arriveDistance = 0.05f;



    private List<Vector2> path;

    private int pathIndex;

    private bool moving;


    private Building targetBuilding;

    public void Initialize(UnitData unitData)
    {
        data = unitData;
    }



    private void OnEnable()
    {
        if (RTSUnitManager.Instance != null)
        {
            RTSUnitManager.Instance.Register(this);
        }
    }



    private void OnDisable()
    {
        if (RTSUnitManager.Instance != null)
        {
            RTSUnitManager.Instance.Unregister(this);
        }
    }



    private void Start()
    {
        FindTargetBuilding();
    }



    // =========================================================
    // 建筑目标
    // =========================================================


    public void OnBuildingChanged()
    {
        if (data == null)
            return;


        Building newTarget =
            BuildingManager.Instance.FindNearest(
                transform.position,
                data.targetBuildingType
            );


        // 找不到目标，寻找 Home
        if (newTarget == null)
        {
            newTarget =
                BuildingManager.Instance.FindNearest(
                    transform.position,
                    BuildingType.Home
                );
        }


        if (newTarget == null)
            return;


        if (newTarget == targetBuilding)
            return;


        targetBuilding = newTarget;

        MoveToBuilding();
    }


    private void FindTargetBuilding()
    {
        if (data == null)
            return;

        if (BuildingManager.Instance == null)
            return;


        Building target =
            BuildingManager.Instance.FindNearest(
                transform.position,
                data.targetBuildingType
            );


        // 没有目标建筑，尝试寻找 Home
        if (target == null &&
            data.targetBuildingType != BuildingType.Home)
        {
            target =
                BuildingManager.Instance.FindNearest(
                    transform.position,
                    BuildingType.Home
                );
        }


        if (target == null)
            return;


        targetBuilding = target;

        MoveToBuilding();
    }



    private void MoveToBuilding()
    {
        if (targetBuilding == null)
            return;


        GridMapManager map =
            GridMapManager.Instance;


        Vector2Int start =
            map.WorldToGrid(
                transform.position
            );


        Vector2Int target =
            targetBuilding.GetNearestWalkableCell(
                start
            );


        MoveTo(
            map.GridToWorld(target)
        );
    }


    private bool InAttackRange()
    {
        if (targetBuilding == null ||
            data == null)
            return false;


        float distance =
            Vector2.Distance(
                transform.position,
                targetBuilding.transform.position
            );


        return distance <= data.attackRange;
    }



    // =========================================================
    // 路径
    // =========================================================


    public void MoveTo(Vector2 target)
    {
        path =
            AStarPathfinder.FindPath(
                transform.position,
                target
            );


        if (path == null ||
            path.Count == 0)
        {
            StopMoving();
            return;
        }



        pathIndex = 0;

        moving = true;


        AdvancePathNode();
    }



    public void OnMapChanged(Vector2Int changedCell)
    {
        if (!moving ||
            path == null)
            return;



        for (int i = pathIndex;
             i < path.Count;
             i++)
        {
            Vector2Int cell =
                GridMapManager.Instance
                .WorldToGrid(path[i]);


            if (cell == changedCell)
            {
                MoveToBuilding();
                return;
            }
        }
    }



    private void Update()
    {
        if (!moving)
            return;


        MoveAlongPath();
    }



    private void MoveAlongPath()
    {
        if (InAttackRange())
        {
            StopMoving();

            // Attack();

            return;
        }



        if (pathIndex >= path.Count)
        {
            StopMoving();
            return;
        }



        Vector2 position =
            transform.position;


        Vector2 target =
            path[pathIndex];



        Vector2 direction =
            target - position;



        if (direction.sqrMagnitude <=
            arriveDistance * arriveDistance)
        {
            AdvancePathNode();
            return;
        }



        Vector2 moveDirection =
            direction.normalized;



        moveDirection +=
            CalculateSeparation()
            *
            separationStrength;



        if (moveDirection.sqrMagnitude > 1)
        {
            moveDirection.Normalize();
        }



        Vector2 movement =
            moveDirection *
            data.moveSpeed *
            Time.deltaTime;



        float distance =
            direction.magnitude;



        if (movement.magnitude > distance)
        {
            movement = direction;
        }



        transform.position +=
            (Vector3)movement;



        UpdateRotation(
            moveDirection
        );
    }



    private void AdvancePathNode()
    {
        Vector2 position =
            transform.position;



        while (pathIndex < path.Count)
        {
            if ((path[pathIndex] -
                 position)
                .sqrMagnitude >
                arriveDistance *
                arriveDistance)
            {
                break;
            }


            pathIndex++;
        }



        if (pathIndex >= path.Count)
        {
            StopMoving();
        }
    }



    // =========================================================
    // 避让
    // =========================================================


    private Vector2 CalculateSeparation()
    {
        if (RTSUnitManager.Instance == null)
            return Vector2.zero;



        List<UnitMover> units =
            RTSUnitManager.Instance.GetUnits();



        Vector2 force =
            Vector2.zero;



        float radiusSqr =
            separationRadius *
            separationRadius;



        for (int i = 0; i < units.Count; i++)
        {
            UnitMover other =
                units[i];


            if (other == this)
                continue;



            Vector2 offset =
                (Vector2)transform.position -
                (Vector2)other.transform.position;



            float distanceSqr =
                offset.sqrMagnitude;



            if (distanceSqr <= 0.0001f ||
                distanceSqr > radiusSqr)
                continue;



            float distance =
                Mathf.Sqrt(distanceSqr);



            float strength =
                1f -
                distance /
                separationRadius;



            force +=
                offset /
                distance *
                strength;
        }



        return force;
    }



    private void UpdateRotation(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.001f)
            return;



        float targetAngle =
            Mathf.Atan2(
                direction.y,
                direction.x
            )
            *
            Mathf.Rad2Deg;



        float currentAngle =
            transform.eulerAngles.z;



        float angle =
            Mathf.MoveTowardsAngle(
                currentAngle,
                targetAngle,
                rotationSpeed *
                Time.deltaTime
            );


        transform.rotation =
            Quaternion.Euler(
                0,
                0,
                angle
            );
    }



    private void StopMoving()
    {
        moving = false;

        path = null;

        pathIndex = 0;
    }



    public bool IsMoving =>
        moving;


    public Building TargetBuilding =>
        targetBuilding;
}