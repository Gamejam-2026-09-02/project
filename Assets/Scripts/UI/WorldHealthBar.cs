using UnityEngine;
using UnityEngine.UI;


public class WorldHealthBar : MonoBehaviour
{
    [SerializeField]
    private Image fill;


    private RectTransform rectTransform;


    private IDamageable target;

    private Transform followTarget;


    private Vector3 worldOffset;


    private float hideTimer;

    private float hideDelay;


    private bool alwaysShow;


    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }



    public void Initialize(
        IDamageable target,
        Transform followTarget,
        Vector3 offset,
        Vector2 size,
        float hideDelay)
    {
        this.target = target;
        this.followTarget = followTarget;
        this.worldOffset = offset;
        this.hideDelay = hideDelay;

        rectTransform.sizeDelta = size;

        rectTransform.localScale = Vector3.one;
    }



    public void SetAlwaysShow(bool value)
    {
        alwaysShow = value;
    }



    public void Show()
    {
        gameObject.SetActive(true);

        Refresh();

        hideTimer = hideDelay;
    }



    public void Refresh()
    {
        if (target == null)
            return;

        fill.fillAmount =
            (float)target.CurrentHealth /
            target.MaxHealth;
    }



    public void Hide()
    {
        HealthBarManager.Instance.Release(this);
    }



    private void LateUpdate()
    {
        if (followTarget == null)
        {
            Hide();
            return;
        }


        transform.position =
            followTarget.position + worldOffset;


        Refresh();


        if (alwaysShow)
            return;


        hideTimer -= Time.deltaTime;


        if (hideTimer <= 0)
        {
            Hide();
        }
    }



    public void Clear()
    {
        target = null;
        followTarget = null;
        worldOffset = Vector3.zero;

        hideTimer = 0;
        hideDelay = 0;

        alwaysShow = false;
    }
}