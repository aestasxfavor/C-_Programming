using System;
using UnityEngine;

public class PlayerCombatStats : MonoBehaviour, IDamageable
{
    [SerializeField] private int startUnitCount = 10;
    [SerializeField] private int attackDamage = 1;

    public int UnitCount { get; private set; }
    public int AttackDamage => attackDamage;

    public event Action<int> OnUnitCountChanged;
    public event Action<int> OnAttackDamageChanged;
    public event Action OnUnitCountZero;

    private void Awake()
    {
        UnitCount = Mathf.Max(0, startUnitCount);

        OnUnitCountChanged?.Invoke(UnitCount);
        OnAttackDamageChanged?.Invoke(attackDamage);
    }

    public void SetUnitCount(int value)
    {
        UnitCount = Mathf.Max(0, value);

        OnUnitCountChanged?.Invoke(UnitCount);

        if (UnitCount <= 0)
        {
            OnUnitCountZero?.Invoke();
        }
    }

    public void AddUnitCount(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        SetUnitCount(UnitCount + amount);
    }

    public void ReduceUnitCount(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        SetUnitCount(UnitCount - amount);
    }

    public void IncreaseAttackDamage(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        attackDamage += amount;

        Debug.Log($"현재 공격력: {attackDamage}");

        OnAttackDamageChanged?.Invoke(attackDamage);
    }

    public void TakeDamage(int damage)
    {
        ReduceUnitCount(damage);
    }
}