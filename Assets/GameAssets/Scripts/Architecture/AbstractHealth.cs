using System;
using UnityEngine;

public abstract class AbstractHealth : MonoBehaviour
{
    private float _health;
    public bool isDead { get; protected set; }
    [SerializeField] protected int _maxHealth = 100;


    protected virtual void Awake()
    {
        _health = _maxHealth;
    }

    public float Health => _health;

    public float MaxHealth => _maxHealth;

    protected virtual void HandleDeath()
    {
        if (isDead) return;
        isDead = true;
        OnDeath?.Invoke();
    }

    public event Action<float> OnHealthChanged;
    public event Action OnDeath;

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
    public Transform Source;
    public Transform Target;

    public DamageInfo(float value, Transform source, Transform target, bool isReflected = false,
        bool isCritical = false)
    {
        Value = value;
        Source = source;
        Target = target;
        IsReflected = isReflected;
        IsCritical = isCritical;
    }
}