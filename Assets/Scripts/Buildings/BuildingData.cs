using System;
using UnityEngine;

[CreateAssetMenu(
    fileName = "BuildingData",
    menuName = "Game/Building Data"
)]
public class BuildingData : ScriptableObject
{
    [Header("基本信息")]
    [SerializeField] private string buildingName;

    [TextArea]
    [SerializeField] private string description;

    public GameObject prefab;

    [Header("防御攻击")]
    public int AttackDamage = 10;

    public float AttackInterval = 1f;

    public float AttackRange = 5f;

    public int MaxHP;

    [Header("类型")]
    public BuildingType Type;

    [Header("占地")]
    [SerializeField]
    private BuildingShapeType shapeType =
        BuildingShapeType.Square;

    [SerializeField] private Vector2Int squareSize = Vector2Int.one;

    [Min(0)]
    [SerializeField] private int circleRadius = 1;


    [Header("建造消耗")]
    [SerializeField] private ResourceCost[] costs;


    public string Name => buildingName;

    public string Description => description;

    public ResourceCost[] Costs => costs;

    public BuildingShapeType ShapeType => shapeType;


    public Vector2Int GetSize()
    {
        switch (shapeType)
        {
            case BuildingShapeType.Square:
                return new Vector2Int(
                    Mathf.Max(1, squareSize.x),
                    Mathf.Max(1, squareSize.y)
                );

            case BuildingShapeType.Circle:
                int diameter = circleRadius * 2 + 1;

                return new Vector2Int(
                    diameter,
                    diameter
                );

            default:
                return Vector2Int.one;
        }
    }


    public Vector2Int[] GetOccupiedCells()
    {
        switch (shapeType)
        {
            case BuildingShapeType.Square:
                return GenerateSquare();

            case BuildingShapeType.Circle:
                return GenerateCircle();

            default:
                return Array.Empty<Vector2Int>();
        }
    }


    private Vector2Int[] GenerateSquare()
    {
        int width = Mathf.Max(1, squareSize.x);
        int height = Mathf.Max(1, squareSize.y);

        Vector2Int[] cells =
            new Vector2Int[width * height];

        int offsetX = width / 2;
        int offsetY = height / 2;

        int index = 0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                cells[index++] = new Vector2Int(
                    x - offsetX,
                    y - offsetY
                );
            }
        }

        return cells;
    }


    private Vector2Int[] GenerateCircle()
    {
        int radius = Mathf.Max(0, circleRadius);
        int diameter = radius * 2 + 1;

        Vector2Int[] cells =
            new Vector2Int[diameter * diameter];

        int count = 0;

        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                if (x * x + y * y <= radius * radius)
                {
                    cells[count++] =
                        new Vector2Int(x, y);
                }
            }
        }

        Array.Resize(ref cells, count);

        return cells;
    }
}


public enum BuildingShapeType
{
    Square,
    Circle
}


[Serializable]
public struct ResourceCost
{
    public ResourceType type;
    public int amount;
}


public enum ResourceType
{
    Gold,
    Wood,
    Stone
}

public enum BuildingType
{
    Home,
    Resource,
    Root,
    Defense
}