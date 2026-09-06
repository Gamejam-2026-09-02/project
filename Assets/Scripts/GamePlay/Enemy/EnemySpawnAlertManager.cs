using DG.Tweening;
using TMPro;
using UnityEngine;

public class EnemySpawnAlertManager : MonoBehaviour
{
    public static EnemySpawnAlertManager Instance { get; private set; }

    [Header("引用")]
    [SerializeField]
    private Camera worldCamera;

    [SerializeField]
    private RectTransform alertContainer;

    [Header("边缘留白（像素）")]
    [SerializeField]
    private float edgePadding = 60f;

    [Header("文字样式")]
    [SerializeField]
    private string alertText = "!";

    [SerializeField]
    private float fontSize = 48f;

    [SerializeField]
    private Color textColor = Color.red;

    [SerializeField]
    private Vector2 textSize = new Vector2(60f, 60f);

    [Header("动画时间")]
    [SerializeField]
    private float scaleInDuration = 0.25f;

    [SerializeField]
    private float displayDuration = 1.2f;

    [SerializeField]
    private float scaleOutDuration = 0.2f;

    [Header("缓动")]
    [SerializeField]
    private Ease scaleInEase = Ease.OutBack;

    [SerializeField]
    private Ease scaleOutEase = Ease.InBack;


    private void Awake()
    {
        Instance = this;

        if (worldCamera == null)
        {
            worldCamera = Camera.main;
        }
    }


    public void ShowAlert(Vector3 spawnWorldPosition)
    {
        if (alertContainer == null || worldCamera == null)
        {
            Debug.LogError("EnemySpawnAlertManager 引用未配置完整");
            return;
        }

        Vector2 screenCenter = new Vector2(
            Screen.width * 0.5f,
            Screen.height * 0.5f
        );

        Vector3 spawnScreenPosition = worldCamera.WorldToScreenPoint(
            spawnWorldPosition
        );

        Vector2 direction = (Vector2)spawnScreenPosition - screenCenter;

        // 生成点与屏幕中心重合的极端情况，给一个默认朝向避免除零
        if (direction.sqrMagnitude < 0.0001f)
        {
            direction = Vector2.up;
        }

        Vector2 edgePoint = GetScreenEdgePoint(screenCenter, direction);

        PlayAlert(edgePoint);
    }


    /// <summary>
    /// 计算从屏幕中心出发、沿指定方向与屏幕边界矩形的交点
    /// </summary>
    private Vector2 GetScreenEdgePoint(Vector2 center, Vector2 direction)
    {
        float halfWidth = Screen.width * 0.5f - edgePadding;
        float halfHeight = Screen.height * 0.5f - edgePadding;

        float scaleToVerticalEdge = direction.x != 0f
            ? halfWidth / Mathf.Abs(direction.x)
            : float.MaxValue;

        float scaleToHorizontalEdge = direction.y != 0f
            ? halfHeight / Mathf.Abs(direction.y)
            : float.MaxValue;

        float scale = Mathf.Min(scaleToVerticalEdge, scaleToHorizontalEdge);

        return center + direction * scale;
    }


    private void PlayAlert(Vector2 screenPosition)
    {
        GameObject alert = new GameObject(
            "SpawnAlert",
            typeof(RectTransform),
            typeof(TextMeshProUGUI)
        );

        RectTransform rect = (RectTransform)alert.transform;
        rect.SetParent(alertContainer, false);
        rect.sizeDelta = textSize;
        rect.position = screenPosition;
        rect.localScale = Vector3.zero;

        TextMeshProUGUI text = alert.GetComponent<TextMeshProUGUI>();
        text.text = alertText;
        text.fontSize = fontSize;
        text.color = textColor;
        text.alignment = TextAlignmentOptions.Center;

        rect.DOScale(Vector3.one, scaleInDuration)
            .SetEase(scaleInEase)
            .OnComplete(() =>
            {
                rect.DOScale(Vector3.zero, scaleOutDuration)
                    .SetDelay(displayDuration)
                    .SetEase(scaleOutEase)
                    .OnComplete(() => Destroy(alert));
            });
    }
}