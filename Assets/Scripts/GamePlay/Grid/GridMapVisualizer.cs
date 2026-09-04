using UnityEngine;

public class GridMapVisualizer : MonoBehaviour
{
    [SerializeField] private GridMapManager gridMap;

    [Header("显示")]
    [SerializeField] private bool showGrid = true;
    [SerializeField] private bool showWalkable = false;
    [SerializeField] private bool showBlocked = true;
    [SerializeField] private bool showOccupied = true;

    [Header("颜色")]
    [SerializeField] private Color gridColor = Color.gray;
    [SerializeField] private Color walkableColor = Color.white;
    [SerializeField] private Color blockedColor = Color.red;
    [SerializeField] private Color occupiedColor = Color.yellow;

    [Header("大小")]
    [Range(0f, 1f)]
    [SerializeField] private float cellFill = 0.8f;

    [Header("运行时")]
    [SerializeField] private Material lineMaterial;

    private Material runtimeMaterial;


    private void Reset()
    {
        gridMap = GetComponent<GridMapManager>();
    }


    private void Awake()
    {
        if (gridMap == null)
            gridMap = GetComponent<GridMapManager>();

        CreateMaterial();
    }


    private void OnDestroy()
    {
        if (runtimeMaterial != null)
        {
            Destroy(runtimeMaterial);
        }
    }

    private void OnRenderObject()
    {
        if (gridMap == null)
            return;

        if (runtimeMaterial == null)
            return;

        DrawRuntime();
    }

    private void DrawRuntime()
    {
        runtimeMaterial.SetPass(0);

        GL.PushMatrix();

        GL.MultMatrix(Matrix4x4.identity);

        if (showGrid)
            DrawGridRuntime();

        DrawCellsRuntime();

        GL.PopMatrix();
    }


    private void DrawCellsRuntime()
    {
        Vector2Int gridSize = gridMap.GridSize;
        float cellSize = gridMap.CellSize;

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector2Int position =
                    new Vector2Int(x, y);

                GridNode node =
                    gridMap.GetNode(position);

                if (node == null)
                    continue;

                if (showOccupied && node.IsOccupied)
                {
                    DrawRuntimeCell(
                        gridMap.GridToWorld(position),
                        cellSize,
                        occupiedColor
                    );

                    continue;
                }

                if (!node.Walkable)
                {
                    if (showBlocked)
                    {
                        DrawRuntimeCell(
                            gridMap.GridToWorld(position),
                            cellSize,
                            blockedColor
                        );
                    }

                    continue;
                }

                if (showWalkable)
                {
                    DrawRuntimeCell(
                        gridMap.GridToWorld(position),
                        cellSize,
                        walkableColor
                    );
                }
            }
        }
    }


    private void DrawRuntimeCell(
        Vector2 center,
        float cellSize,
        Color color)
    {
        float size =
            cellSize * cellFill;

        float half =
            size * 0.5f;

        GL.Begin(GL.QUADS);

        GL.Color(color);

        GL.Vertex3(
            center.x - half,
            center.y - half,
            0f
        );

        GL.Vertex3(
            center.x + half,
            center.y - half,
            0f
        );

        GL.Vertex3(
            center.x + half,
            center.y + half,
            0f
        );

        GL.Vertex3(
            center.x - half,
            center.y + half,
            0f
        );

        GL.End();
    }


    private void DrawGridRuntime()
    {
        Vector2Int gridSize =
            gridMap.GridSize;

        float cellSize =
            gridMap.CellSize;

        Vector2 bottomLeft =
            gridMap.MapCenter -
            new Vector2(
                gridSize.x * cellSize,
                gridSize.y * cellSize
            ) * 0.5f;


        GL.Begin(GL.LINES);

        GL.Color(gridColor);


        // 横线
        for (int y = 0; y <= gridSize.y; y++)
        {
            Vector2 start =
                bottomLeft +
                Vector2.up * y * cellSize;

            Vector2 end =
                start +
                Vector2.right *
                gridSize.x *
                cellSize;

            GL.Vertex3(
                start.x,
                start.y,
                0f
            );

            GL.Vertex3(
                end.x,
                end.y,
                0f
            );
        }


        // 竖线
        for (int x = 0; x <= gridSize.x; x++)
        {
            Vector2 start =
                bottomLeft +
                Vector2.right * x * cellSize;

            Vector2 end =
                start +
                Vector2.up *
                gridSize.y *
                cellSize;

            GL.Vertex3(
                start.x,
                start.y,
                0f
            );

            GL.Vertex3(
                end.x,
                end.y,
                0f
            );
        }

        GL.End();
    }


    private void CreateMaterial()
    {
        if (lineMaterial != null)
            return;

        Shader shader =
            Shader.Find("Hidden/Internal-Colored");

        if (shader == null)
            return;

        runtimeMaterial =
            new Material(shader);

        runtimeMaterial.hideFlags =
            HideFlags.HideAndDontSave;

        runtimeMaterial.SetInt(
            "_SrcBlend",
            (int)UnityEngine.Rendering.BlendMode.SrcAlpha
        );

        runtimeMaterial.SetInt(
            "_DstBlend",
            (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha
        );

        runtimeMaterial.SetInt(
            "_Cull",
            (int)UnityEngine.Rendering.CullMode.Off
        );

        runtimeMaterial.SetInt(
            "_ZWrite",
            0
        );
    }
}