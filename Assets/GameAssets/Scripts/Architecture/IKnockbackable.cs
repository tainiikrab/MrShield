using UnityEngine;

public interface IKnockbackable
{
    public void ApplyKnockback(Vector3 direction, float force, DamageInfo damageInfo);
}