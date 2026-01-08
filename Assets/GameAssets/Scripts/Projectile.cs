using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float lifeTime;

    [SerializeField] private Vector3 forwardAxis = Vector3.forward;
    [SerializeField] private LayerMask layerMask;

    [SerializeField] private float raycastLength;

    private Transform _target;
    private Shooter _shooter;
    private Vector3 _velocity;

    private Vector3 _startPos;
    private Vector3 _gravity;

    [SerializeField] private float sphereRadius = 0.1f;
    private Vector3 _prevPos;

    public void Initialize(Transform target, Shooter shooter, Vector3 initialVelocity)
    {
        _time = 0f;
        _startPos = transform.position;
        _target = target;
        _shooter = shooter;
        _velocity = initialVelocity;
        StartCoroutine(DestroyProjectile());
        _prevPos = transform.position;
        _gravity = Physics.gravity;
    }

    private float _time = 0f;

    private void Update()
    {
        _time += Time.deltaTime;

        var gravity = _gravity;
        var newPos =
            _startPos +
            _velocity * _time +
            0.5f * _time * _time * gravity;

        var delta = newPos - _prevPos;
        var dist = delta.magnitude;

        if (dist > 0f)
            if (Physics.SphereCast(_prevPos, sphereRadius, delta / dist, out var hit, dist + 0.01f, layerMask,
                    QueryTriggerInteraction.Ignore))
                OnHit(hit);

        transform.position = newPos;
        _prevPos = newPos;

        RotateAlongPath();
    }


    private void RotateAlongPath()
    {
        var velocityNow = _velocity + _gravity * _time;

        if (velocityNow.sqrMagnitude < 0.0001f)
            return;

        var lookRotation = Quaternion.LookRotation(velocityNow.normalized);

        if (forwardAxis != Vector3.forward)
        {
            var axisCorrection =
                Quaternion.FromToRotation(forwardAxis, Vector3.forward);
            lookRotation *= Quaternion.Inverse(axisCorrection);
        }

        transform.rotation = lookRotation;
    }

    private void OnHit(RaycastHit hit)
    {
        var reflector = hit.transform.GetComponentInParent<IProjectileReflector>();
        if (reflector?.IsReflecting ?? false)
        {
            _shooter.ReflectProjectile(this);
            return;
        }

        var health = hit.transform.GetComponentInParent<AbstractHealth>();
        health?.TakeDamage(10);

        Destroy(gameObject);
    }

    private IEnumerator DestroyProjectile()
    {
        yield return new WaitForSeconds(lifeTime);
        _shooter.RemoveProjectile(this);
        Destroy(gameObject);
    }
}