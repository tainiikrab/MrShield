using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float lifeTime;

    [SerializeField] private Vector3 forwardAxis = Vector3.forward;
    [SerializeField] private LayerMask layerMask;

    private Transform _target;
    private Shooter _shooter;
    private Vector3 _velocity;

    private Vector3 _startPos;
    private Vector3 _gravity;

    public void Initialize(Transform target, Shooter shooter, Vector3 initialVelocity)
    {
        _startPos = transform.position;
        _target = target;
        _shooter = shooter;
        _velocity = initialVelocity;
        StartCoroutine(DestroyProjectile());
        _gravity = Physics.gravity;
    }

    private float time = 0f;

    private void Update()
    {
        time += Time.deltaTime;

        var gravity = Physics.gravity;
        var position =
            _startPos +
            _velocity * time +
            0.5f * time * time * gravity;

        var delta = position - transform.position;
        transform.position = position;

        RotateAlongPath();

        if (Physics.Raycast(transform.position - delta, delta.normalized, out var hit, delta.magnitude,
                layerMask)) OnHit(hit);
    }

    private void RotateAlongPath()
    {
        var velocityNow = _velocity + _gravity * time;

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
        var health = hit.transform.GetComponentInParent<AbstractHealth>();
        health?.TakeDamage(10);
        Debug.Log(hit.transform.name);
        Destroy(gameObject);
    }

    private IEnumerator DestroyProjectile()
    {
        yield return new WaitForSeconds(lifeTime);
        _shooter.RemoveProjectile(this);
        Destroy(gameObject);
    }
}