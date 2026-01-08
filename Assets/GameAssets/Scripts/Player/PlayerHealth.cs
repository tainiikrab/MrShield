using UnityEngine;

public class PlayerHealth : AbstractHealth
{
    protected override void HandleDeath()
    {
        if (isDead) return;
        isDead = true;
        Debug.Log("Player died");
    }
}