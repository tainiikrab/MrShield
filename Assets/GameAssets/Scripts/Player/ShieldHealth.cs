using UnityEngine;

public class ShieldHealth : AbstractHealth
{
    protected override void HandleDeath()
    {
        if (isDead) return;
        Debug.Log("Shield destroyed");
        isDead = true;
    }

    public override void CalculateDamage(in DamageInfo damageInfo)
    {
        var damage = damageInfo.Value;
    }
}