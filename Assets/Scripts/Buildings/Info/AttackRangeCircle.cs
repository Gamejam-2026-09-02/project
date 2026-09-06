using UnityEngine;


[RequireComponent(typeof(LineRenderer))]
public class AttackRangeCircle : MonoBehaviour
{
    [Header("圆圈精度")]
    [SerializeField]
    private int segments = 80;


    [Header("线条粗细")]
    [SerializeField]
    private float lineWidth = 0.08f;


    private LineRenderer line;



    private void Awake()
    {
        line = GetComponent<LineRenderer>();

        line.loop = true;
        line.useWorldSpace = true;

        line.startWidth = lineWidth;
        line.endWidth = lineWidth;

        // 保证显示在建筑上层
        line.sortingOrder = 100;

        line.enabled = false;
    }



    /// <summary>
    /// 显示攻击范围
    /// </summary>
    public void Show(
        Vector3 worldPosition,
        float radius)
    {
        // 攻击范围为0，不显示
        if (radius <= 0)
        {
            Hide();
            return;
        }


        Draw(
            worldPosition,
            radius
        );


        line.enabled = true;
    }



    /// <summary>
    /// 隐藏攻击范围
    /// </summary>
    public void Hide()
    {
        line.enabled = false;
    }



    private void Draw(
        Vector3 center,
        float radius)
    {
        line.positionCount = segments;


        for (int i = 0; i < segments; i++)
        {
            float angle =
                Mathf.PI * 2f *
                i /
                segments;


            Vector3 point =
                center +
                new Vector3(
                    Mathf.Cos(angle),
                    Mathf.Sin(angle),
                    0
                )
                *
                radius;


            line.SetPosition(
                i,
                point
            );
        }
    }
}