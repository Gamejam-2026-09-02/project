using System;
using UnityEngine;

public class UnitHealth : MonoBehaviour, IDamageable
{
    public UnitData data;


    private int currentHealth;



    public int CurrentHealth =>
        currentHealth;


    public int MaxHealth =>
        data.maxHealth;

    public event Action<int> OnDamaged;

    public event Action OnDeath;


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