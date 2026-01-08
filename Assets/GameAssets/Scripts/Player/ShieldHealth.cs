using UnityEngine;

public class ShieldHealth : AbstractHealth
{
    protected override void HandleDeath()
    {
        if (isDead) return;
        Debug.Log("Shield destroyed");
        isDead = true;
    }
}