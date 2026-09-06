using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HitEffect : MonoBehaviour
{
    [Header("效果参数")]
    [SerializeField] private float redDuration = 0.08f;
    [SerializeField] private float flashDuration = 0.3f;
    [SerializeField] private float flashSpeed = 20f;

    [Header("颜色")]
    [SerializeField] private Color hitColor = Color.red;


    private SpriteRenderer spriteRenderer;
    private Image image;

    private Color originalColor;
    private Coroutine effectCoroutine;


    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        image = GetComponent<Image>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
        else if (image != null)
            originalColor = image.color;
    }


    /// <summary>
    /// 父物体调用
    /// </summary>
    public void PlayHitEffect()
    {
        if (effectCoroutine != null)
            StopCoroutine(effectCoroutine);

        effectCoroutine = StartCoroutine(HitRoutine());
    }


    private IEnumerator HitRoutine()
    {
        // 红色瞬间覆盖
        SetColor(hitColor);

        yield return new WaitForSeconds(redDuration);


        // 红色恢复
        float timer = 0;

        while (timer < flashDuration)
        {
            timer += Time.deltaTime;

            float alpha = Mathf.Abs(Mathf.Sin(timer * flashSpeed));

            Color color = originalColor;
            color.a *= alpha;

            SetColor(color);

            yield return null;
        }


        // 确保恢复
        SetColor(originalColor);

        effectCoroutine = null;
    }


    private void SetColor(Color color)
    {
        if (spriteRenderer != null)
            spriteRenderer.color = color;

        if (image != null)
            image.color = color;
    }
}