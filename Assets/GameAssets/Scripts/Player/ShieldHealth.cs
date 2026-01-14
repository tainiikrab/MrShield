using UnityEngine;

public class ShieldHealth : AbstractHealth
{
    [Range(0, 1)] [SerializeField] private float reflectionDamageMitigation = 0.9f;

    protected override void HandleDeath()
    {
        base.HandleDeath();
        Debug.Log("Shield destroyed");
    }

    public override void CalculateDamage(in DamageInfo damageInfo)
    {
        var damage = damageInfo.Value;
        if (damageInfo.IsReflected) damage *= 1 - reflectionDamageMitigation;
        // Debug.Log("Shield reflected damage");
        ApplyDamage(damage);
    }
}