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

    [Header("限制")]
    [SerializeField] private bool limitMovement = false;
    [SerializeField] private Vector2 minPosition;
    [SerializeField] private Vector2 maxPosition;

    private Vector2 currentVelocity;

    private void Update()
    {
        Vector2 input = GetInput();

        Vector2 targetVelocity =
            input * moveSpeed;

        float rate = input.sqrMagnitude > 0.01f
            ? acceleration
            : deceleration;

        currentVelocity = Vector2.MoveTowards(
            currentVelocity,
            targetVelocity,
            rate * Time.deltaTime
        );

        Vector3 movement =
            new Vector3(
                currentVelocity.x,
                currentVelocity.y,
                0f
            ) * Time.deltaTime;

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
            Vector3 mousePosition =
                Input.mousePosition;

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