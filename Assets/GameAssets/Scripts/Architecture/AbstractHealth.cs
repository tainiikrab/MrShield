using System;
using UnityEngine;

public abstract class AbstractHealth : MonoBehaviour
{
    private int _health;
    [SerializeField] private int _maxHealth = 100;

    protected void InitializeHealth()
    {
        _health = _maxHealth;
    }

    protected void Awake()
    {
        InitializeHealth();
    }

    public int Health => _health;

    public int MaxHealth => _maxHealth;

    protected abstract void HandleDeath();

    public event Action<int> OnDamaged;

    public virtual void TakeDamage(int damage)
    {
        _health = Mathf.Max(_health - damage, 0);
        OnDamaged?.Invoke(_health);
        if (_health <= 0) HandleDeath();
    }
}