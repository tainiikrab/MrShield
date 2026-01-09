using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class WalkingEnemy : AbstractDamageDealer
{
    [SerializeField] private Transform target;
    [SerializeField] private float speed;
    [SerializeField] private Transform _body;
    private CharacterController _characterController;

    [SerializeField] private float randomSpeedDeviation;
    [SerializeField] private float randomDirectionDeviation;

    // new:
    [SerializeField] private LayerMask damageableLayers;
    [SerializeField] private float contactCooldown = 0.5f;
    private readonly Dictionary<Transform, float> _lastHitTime = new();

    [SerializeField] private float knockbackDamping = 6f;
    [SerializeField] private float minKnockbackThreshold = 0.05f;
    private Vector3 _knockbackVelocity;

    private Vector3 _directionDeviation;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        speed += Random.Range(-randomSpeedDeviation, randomSpeedDeviation);
        _directionDeviation = Random.insideUnitSphere * randomDirectionDeviation;
    }

    private Vector3 _direction;

    private void Update()
    {
        _direction = (target.position + _directionDeviation - transform.position).normalized;
        _body.LookAt(target.position + _directionDeviation);
        _characterController.SimpleMove(speed * _direction);

        if (_knockbackVelocity.sqrMagnitude > minKnockbackThreshold * minKnockbackThreshold)
        {
            _characterController.Move(_knockbackVelocity * Time.deltaTime);
            _knockbackVelocity = Vector3.Lerp(_knockbackVelocity, Vector3.zero, knockbackDamping * Time.deltaTime);
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.transform.IsChildOf(transform)) return;

        var hitLayerMask = 1 << hit.gameObject.layer;
        var allowed = (damageableLayers.value & hitLayerMask) != 0;
        if (!allowed) return;

        var health = hit.collider.GetComponentInParent<AbstractHealth>();
        if (health == null) return;

        var damageInfo = new DamageInfo(_damage);

        var shield = hit.collider.GetComponentInParent<IShield>();
        if (shield != null && shield.IsReflecting)
        {
            var shieldT = hit.collider.transform;
            var shieldForward = shieldT.forward;

            shieldForward.y = 0f;
            if (shieldForward.sqrMagnitude < 0.0001f)
            {
                shieldForward = (transform.position - hit.collider.transform.position).normalized;
                shieldForward.y = 0f;
            }

            shieldForward.Normalize();

            // var side = Mathf.Sign(Vector3.Dot(transform.position - shieldT.position, shieldForward));
            // var knockbackDir = shieldForward * side;
            var knockbackDir = shieldForward;

            var force = shield.KnockbackForce;
            _knockbackVelocity = knockbackDir * force;

            damageInfo.IsReflected = true;
        }

        var t = health.transform;
        if (_lastHitTime.TryGetValue(t, out var last) && Time.time - last < contactCooldown) return;

        DealDamage(health, damageInfo);

        _lastHitTime[t] = Time.time;
    }
}