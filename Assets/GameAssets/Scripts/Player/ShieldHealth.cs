using UnityEngine;

public class ShieldHealth : AbstractHealth
{
    protected override void HandleDeath()
    {
        Debug.Log("Shield destroyed");
    }
}