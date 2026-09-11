using UnityEngine;


public class HealthBarController : MonoBehaviour
{
    [SerializeField]
    private Vector2 barSize = new Vector2(40, 6);


    [SerializeField]
    private float hideDelay = 2f;

    [Header("œ‘ æ")]
    [SerializeField]
    private bool alwaysShowWhenNotFullHealth;


    [Header("Œª÷√∆´“∆")]
    [SerializeField]
    private float bottomOffset = 0.2f;


    private IDamageable damageable;

    private WorldHealthBar bar;


    private void Awake()
    {
        damageable = GetComponent<IDamageable>();
    }


    private void OnEnable()
    {
        damageable.OnDamaged += OnDamaged;
        damageable.OnDeath += OnDeath;
    }


    private void OnDisable()
    {
        if (damageable == null)
            return;

        damageable.OnDamaged -= OnDamaged;
        damageable.OnDeath -= OnDeath;
    }


    private void OnDamaged(int damage)
    {
        if (bar == null)
        {
            bar = HealthBarManager.Instance.Get();

            Vector3 offset =
                Vector3.down * GetHalfHeight();


            bar.Initialize(
                damageable,
                transform,
                offset,
                barSize,
                hideDelay
            );

            bar.SetAlwaysShow(alwaysShowWhenNotFullHealth);
        }

        bar.Show();
    }


    private float GetHalfHeight()
    {
        SpriteRenderer renderer =
            GetComponentInChildren<SpriteRenderer>();

        if (renderer != null)
        {
            return renderer.bounds.extents.y + bottomOffset;
        }


        Collider2D col =
            GetComponentInChildren<Collider2D>();

        if (col != null)
        {
            return col.bounds.extents.y + bottomOffset;
        }


        return bottomOffset;
    }


    private void OnDeath()
    {
        if (bar != null)
        {
            bar.Clear();
            bar = null;
        }
    }
}