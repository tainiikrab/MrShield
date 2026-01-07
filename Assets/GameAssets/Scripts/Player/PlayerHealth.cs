using UnityEngine;

public class PlayerHealth : AbstractHealth
{
    protected override void HandleDeath()
    {
        Debug.Log("Player died");
    }
}