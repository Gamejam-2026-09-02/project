using System.Collections.Generic;
using UnityEngine;


public class UnitMover : MonoBehaviour
{
    [Header("单位数据")]
    [HideInInspector]
    public UnitData data;


    [Header("转向")]
    [SerializeField]
    private float rotationSpeed = 720f;


    [SerializeField]
    private float arriveDistance = 0.05f;


    [Header("Home危险范围")]
    [SerializeField]
    private float homeDangerRange = 8f;


    [SerializeField]
    private float homeCheckInterval = 0.2f;



    private List<Vector2> path;

    private int pathIndex;

    private bool moving;


    private Building targetBuilding;

    private Building homeBuilding;


    private float homeCheckTimer;


    private void Awake()
    {
        data = GetComponent<UnitCore>().data;
    }

    //public void Initialize(UnitData unitData)
    //{
    //    data = unitData;
    //}



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
        CacheHome();

        FindTargetBuilding();
    }



    // =========================================================
    // 目标选择
    // =========================================================


    public void OnBuildingChanged()
    {
        if (data == null)
            return;


        Building newTarget;

        Debug.Log("newTarget");

        if (IsHomeInDangerRange())
        {
            newTarget = homeBuilding;
        }
        else
        {
            newTarget =
                BuildingManager.Instance.FindNearest(
                    transform.position,
                    data.targetBuildingType
                );


            if (newTarget == null)
            {
                newTarget =
                    BuildingManager.Instance.FindNearest(
                        transform.position,
                        BuildingType.Home
                    );
            }
        }



        if (newTarget == null)
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



        Building target;



        if (IsHomeInDangerRange())
        {
            target = homeBuilding;
        }
        else
        {
            target =
                BuildingManager.Instance.FindNearest(
                    transform.position,
                    data.targetBuildingType
                );


            if (target == null &&
                data.targetBuildingType != BuildingType.Home)
            {
                target =
                    BuildingManager.Instance.FindNearest(
                        transform.position,
                        BuildingType.Home
                    );
            }
        }



        if (target == null)
            return;



        targetBuilding = target;


        MoveToBuilding();
    }



    private void CacheHome()
    {
        if (homeBuilding != null)
            return;


        if (BuildingManager.Instance == null)
            return;


        homeBuilding =
            BuildingManager.Instance.FindNearest(
                transform.position,
                BuildingType.Home
            );
    }



    private bool IsHomeInDangerRange()
    {
        if (homeBuilding == null)
            return false;


        float distance =
            Vector2.Distance(
                transform.position,
                homeBuilding.transform.position
            );


        return distance <= homeDangerRange;
    }



    private void CheckHomeDanger()
    {
        if (homeBuilding == null)
            return;


        homeCheckTimer -= Time.deltaTime;


        if (homeCheckTimer > 0)
            return;


        homeCheckTimer = homeCheckInterval;



        if (IsHomeInDangerRange())
        {
            if (targetBuilding != homeBuilding)
            {
                targetBuilding = homeBuilding;

                MoveToBuilding();
            }
        }
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
        CheckHomeDanger();


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



        Vector2 movement =
            moveDirection *
            Mathf.Min(
                data.moveSpeed * Time.deltaTime,
                direction.magnitude
            );



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
            if ((path[pathIndex] - position)
                .sqrMagnitude >
                arriveDistance * arriveDistance)
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