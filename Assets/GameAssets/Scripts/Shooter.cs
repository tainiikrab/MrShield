using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooter : MonoBehaviour
{
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private float launchSpeed = 20f;
    [SerializeField] private bool highArc = true;

    private List<Projectile> _projectiles = new();
    public Transform target;
    [SerializeField] private float shootDelay = 1f;

    private void Start()
    {
        StartCoroutine(ShootingCoroutine());
    }

    private IEnumerator ShootingCoroutine()
    {
        while (true)
        {
            Shoot();
            yield return new WaitForSeconds(shootDelay);
        }
    }


    public void Shoot()
    {
        if (target == null || projectilePrefab == null) return;

        var projectile = Instantiate(projectilePrefab, transform.position, transform.rotation);
        _projectiles.Add(projectile);

        if (TryCalculateLaunchVelocity(transform.position, target.position, launchSpeed, highArc, out var velocity))
        {
            projectile.Initialize(target, this, velocity);
        }
        else
        {
            var dir = (target.position - transform.position).normalized;
            projectile.Initialize(target, this, dir * launchSpeed);
        }
    }

    private bool TryCalculateLaunchVelocity(Vector3 start, Vector3 target, float speed, bool highArc,
        out Vector3 result)
    {
        result = Vector3.zero;
        var toTarget = target - start;
        var toTargetXZ = new Vector3(toTarget.x, 0f, toTarget.z);

        var x = toTargetXZ.magnitude;
        var y = toTarget.y;
        var g = Mathf.Abs(Physics.gravity.y);

        if (x < 0.001f)
        {
            if (Mathf.Approximately(speed, 0f)) return false;
            result = Vector3.up * Mathf.Sign(y) * speed;
            return true;
        }

        var v2 = speed * speed;
        var v4 = v2 * v2;
        var discr = v4 - g * (g * x * x + 2f * y * v2);

        if (discr < 0f) return false;

        var sqrtDisc = Mathf.Sqrt(discr);
        var numerator = v2 + (highArc ? sqrtDisc : -sqrtDisc);
        var angle = Mathf.Atan2(numerator, g * x);

        var vxz = Mathf.Cos(angle) * speed;
        var dirXZ = toTargetXZ.normalized;
        result = dirXZ * vxz + Vector3.up * Mathf.Sin(angle) * speed;
        return true;
    }

    public void RemoveProjectile(Projectile projectile)
    {
        _projectiles.Remove(projectile);
    }
}