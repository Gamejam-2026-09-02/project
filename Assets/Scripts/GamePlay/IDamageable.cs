using System;

public interface IDamageable
{
    int CurrentHealth { get; }

    int MaxHealth { get; }


    event Action<int> OnDamaged;

    event Action OnDeath;
}