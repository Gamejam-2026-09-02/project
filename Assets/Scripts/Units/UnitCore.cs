using System;
using UnityEngine;

public class UnitCore : MonoBehaviour, IDamageable
{
    public UnitData data;


    private int currentHealth;



    public int CurrentHealth =>
        currentHealth;


    public int MaxHealth =>
        data.maxHealth;

    public event Action<int> OnDamaged;

    public event Action OnDeath;

    private HitEffect hitEffect;

    private void Awake()
    {
        hitEffect = GetComponentInChildren<HitEffect>();
    }



    public void Initialize(UnitData unitData)
    {
        data = unitData;

        currentHealth = data.maxHealth;
    }



    public virtual void TakeDamage(
      int damage)
    {
        if (damage <= 0)
            return;


        currentHealth -= damage;

        hitEffect.PlayHitEffect();
        OnDamaged?.Invoke(damage);



        if (currentHealth <= 0)
        {
            OnDeath?.Invoke();

            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}