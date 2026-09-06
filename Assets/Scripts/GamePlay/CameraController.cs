using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("移动")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float acceleration = 30f;
    [SerializeField] private float deceleration = 40f;

    [Header("鼠标边缘")]
    [SerializeField] private bool edgeMoveEnabled = true;

    [Tooltip("鼠标距离屏幕边缘多少像素开始移动")]
    [SerializeField] private float edgeSize = 30f;

    [Header("缩放")]
    [SerializeField] private float zoomSpeed = 5f;

    [Tooltip("最小摄像机 Size，数值越小视野越近")]
    [SerializeField] private float minZoom = 3f;

    [Tooltip("最大摄像机 Size，数值越大视野越远")]
    [SerializeField] private float maxZoom = 10f;

    [Header("限制")]
    [SerializeField] private bool limitMovement = false;
    [SerializeField] private Vector2 minPosition;
    [SerializeField] private Vector2 maxPosition;

    private Vector2 currentVelocity;
    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Update()
    {
        //if (GameStateController.Instance.IsPaused)
        //    return;

        UpdateMovement();
        UpdateZoom();
    }

    private void UpdateMovement()
    {
        Vector2 input = GetInput();

        Vector2 targetVelocity = input * moveSpeed;

        float rate = input.sqrMagnitude > 0.01f
            ? acceleration
            : deceleration;

        currentVelocity = Vector2.MoveTowards(
            currentVelocity,
            targetVelocity,
            rate * Time.unscaledDeltaTime
        );

        Vector3 movement = new Vector3(
            currentVelocity.x,
            currentVelocity.y,
            0f
        ) * Time.unscaledDeltaTime;

        transform.position += movement;

        if (limitMovement)
        {
            Vector3 position = transform.position;

            position.x = Mathf.Clamp(
                position.x,
                minPosition.x,
                maxPosition.x
            );

            position.y = Mathf.Clamp(
                position.y,
                minPosition.y,
                maxPosition.y
            );

            transform.position = position;
        }
    }

    private void UpdateZoom()
    {
        float scroll = Input.mouseScrollDelta.y;

        if (Mathf.Abs(scroll) < 0.01f)
            return;

        cam.orthographicSize = Mathf.Clamp(
            cam.orthographicSize - scroll * zoomSpeed,
            minZoom,
            maxZoom
        );
    }

    private Vector2 GetInput()
    {
        Vector2 input = Vector2.zero;

        // WASD
        if (Input.GetKey(KeyCode.W))
            input.y += 1f;

        if (Input.GetKey(KeyCode.S))
            input.y -= 1f;

        if (Input.GetKey(KeyCode.A))
            input.x -= 1f;

        if (Input.GetKey(KeyCode.D))
            input.x += 1f;

        // 方向键
        if (Input.GetKey(KeyCode.UpArrow))
            input.y += 1f;

        if (Input.GetKey(KeyCode.DownArrow))
            input.y -= 1f;

        if (Input.GetKey(KeyCode.LeftArrow))
            input.x -= 1f;

        if (Input.GetKey(KeyCode.RightArrow))
            input.x += 1f;

        // 鼠标屏幕边缘移动
        if (edgeMoveEnabled)
        {
            Vector3 mousePosition = Input.mousePosition;

            if (mousePosition.x <= edgeSize)
                input.x -= 1f;
            else if (mousePosition.x >= Screen.width - edgeSize)
                input.x += 1f;

            if (mousePosition.y <= edgeSize)
                input.y -= 1f;
            else if (mousePosition.y >= Screen.height - edgeSize)
                input.y += 1f;
        }

        return Vector2.ClampMagnitude(input, 1f);
    }
}