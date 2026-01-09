using UnityEngine;

public abstract class AbstractDamageDealer : MonoBehaviour
{
    [SerializeField] protected float _damage;
    public float Damage => _damage;

    public virtual void DealDamage(AbstractHealth health, DamageInfo damageInfo)
    {
        if (health == null)
        {
            Debug.LogWarning("Health is null");
            return;
        }

        health.CalculateDamage(damageInfo);
    }

    public virtual void SetDamage(int damage)
    {
        _damage = damage;
    }
}