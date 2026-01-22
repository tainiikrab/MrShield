using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using VContainer;
using Random = UnityEngine.Random;

public class ShootingEnemyController : MonoBehaviour, IHasTarget
{
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private float launchSpeed = 20f;
    [SerializeField] private bool highArc = true;

    [SerializeField] private Transform shootingPoint;

    private List<Projectile> _projectiles = new();
    [Inject] public Transform Target { get; set; }
    [SerializeField] private float shootDelay = 1f;

    [SerializeField] private float minimalWalkingDistance = 5f;
    [SerializeField] private float maximalWalkingDistance = 10f;

    [SerializeField] private float walkingSpeed = 5f;

    private float _walkingDistance;


    private void Awake()
    {
        _walkingDistance = Random.Range(minimalWalkingDistance, maximalWalkingDistance);
        walkingSpeed += Random.Range(-1f, 1f);
    }

    private void Start()
    {
        Debug.Log(Target.name);
        StartCoroutine(WalkingCoroutine());
    }

    private float _walkedDistance;

    private IEnumerator WalkingCoroutine()
    {
        while (true)
        {
            var dist = walkingSpeed * Time.deltaTime;
            transform.position += transform.forward * dist;
            _walkedDistance += dist;
            if (_walkedDistance >= _walkingDistance)
            {
                StartCoroutine(ShootingCoroutine());
                break;
            }

            yield return null;
        }
    }

    private IEnumerator ShootingCoroutine()
    {
        while (true)
        {
            Shoot(Target, launchSpeed);
            yield return new WaitForSeconds(shootDelay);
        }
    }


    public void Shoot(Transform target, float speed, Projectile projectile = null)
    {
        if (target == null)
        {
            Debug.LogWarning("No target");
            return;
        }

        if (projectilePrefab == null)
        {
            Debug.LogWarning("No projectile prefab");
            return;
        }

        if (projectile == null)
        {
            projectile = Instantiate(projectilePrefab, shootingPoint.position, transform.rotation);
            _projectiles.Add(projectile);
        }

        if (TryCalculateLaunchVelocity(projectile.transform.position, target.position, speed, highArc,
                out var velocity))
        {
            projectile.Initialize(target, this, velocity);
        }
        else
        {
            var dir = (target.position - transform.position).normalized;
            projectile.Initialize(target, this, dir * speed);
        }
    }

    public void ReflectProjectile(Projectile projectile)
    {
        Shoot(transform, launchSpeed, projectile);
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