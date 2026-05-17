using System;
using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 10;

    private int currentHealth;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => currentHealth <= 0;

    public event Action OnDied;
    public event Action<int, int> OnHealthChanged;

    private void OnEnable()
    {
        ResetHealth();
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(int damage)
    {
        if (IsDead)
        {
            return;
        }

        if (damage <= 0)
        {
            return;
        }

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            OnDied?.Invoke();
        }
    }
}