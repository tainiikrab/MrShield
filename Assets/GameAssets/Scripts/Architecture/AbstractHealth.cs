using System;
using UnityEngine;

public abstract class AbstractHealth : MonoBehaviour
{
    private float _health;
    public bool isDead { get; protected set; }
    [SerializeField] private int _maxHealth = 100;

    protected void InitializeHealth()
    {
        _health = _maxHealth;
    }

    protected void Awake()
    {
        InitializeHealth();
    }

    public float Health => _health;

    public float MaxHealth => _maxHealth;

    protected abstract void HandleDeath();

    public event Action<float> OnHealthChanged;

    public abstract void CalculateDamage(in DamageInfo damageInfo);

    protected virtual void ApplyDamage(float finalDamage)
    {
        if (isDead) return;
        _health = Mathf.Max(_health - finalDamage, 0);
        OnHealthChanged?.Invoke(_health);
        if (_health <= 0) HandleDeath();
    }
}

public struct DamageInfo
{
    public float Value;
    public bool IsReflected;
    public bool IsCritical;

    public DamageInfo(float value, bool isReflected = false, bool isCritical = false)
    {
        Value = value;
        IsReflected = isReflected;
        IsCritical = isCritical;
    }
}