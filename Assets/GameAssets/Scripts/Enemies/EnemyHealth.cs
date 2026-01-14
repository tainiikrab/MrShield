using UnityEngine;

public class EnemyHealth : AbstractHealth
{
    protected override void HandleDeath()
    {
        base.HandleDeath();
        Destroy(gameObject);
    }

    public override void CalculateDamage(in DamageInfo damageInfo)
    {
        ApplyDamage(damageInfo.Value);
    }
}