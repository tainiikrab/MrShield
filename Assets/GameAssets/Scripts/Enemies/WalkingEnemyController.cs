using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(CharacterController))]
public class WalkingEnemyController : AbstractDamageDealer, IHasTarget
{
    [Header("Movement")] public Transform Target { get; set; }
    [SerializeField] private float speed;
    [SerializeField] private Transform _body;
    [SerializeField] private float randomSpeedDeviation;
    [SerializeField] private float randomDirectionDeviation;

    [Space] [Header("Combat")] [SerializeField]
    private LayerMask damageableLayers;

    [SerializeField] private float contactCooldown = 0.5f;
    [SerializeField] private float damageDelay = 0.2f;

    [Space] [Header("Knockback")] [SerializeField]
    private float knockbackDamping = 6f;

    [SerializeField] private float minKnockbackThreshold = 0.05f;
    [SerializeField] private float knockbackPropagationForce = 0.7f;

    private Vector3 _knockbackVelocity;
    private Vector3 _directionDeviation;
    private CharacterController _characterController;

    private readonly Dictionary<Transform, float> _lastHitTime = new();
    private readonly Dictionary<Transform, CancellationTokenSource> _pendingDamageCts = new();

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        speed += Random.Range(-randomSpeedDeviation, randomSpeedDeviation);
        _directionDeviation = Random.insideUnitSphere * randomDirectionDeviation;
    }

    private Vector3 _direction;

    private void Update()
    {
        _direction = (Target.position + _directionDeviation - transform.position).normalized;
        _body.LookAt(Target.position + _directionDeviation);
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

        if (_knockbackVelocity.sqrMagnitude > minKnockbackThreshold * minKnockbackThreshold)
        {
            var otherEnemy = hit.collider.GetComponentInParent<WalkingEnemyController>();
            if (otherEnemy != null && otherEnemy != this) PropagateKnockbackToEnemy(otherEnemy);
        }

        var hitLayerMask = 1 << hit.gameObject.layer;
        var allowed = (damageableLayers.value & hitLayerMask) != 0;
        if (!allowed) return;

        var health = hit.collider.GetComponentInParent<AbstractHealth>();
        if (health == null) return;

        var damageInfo = new DamageInfo(_damage);

        var shield = hit.collider.GetComponentInParent<IShield>();

        if (shield != null && shield.IsReflecting) CalculateShieldKnockback(shield, ref damageInfo);

        var t = health.transform;
        if (_lastHitTime.TryGetValue(t, out var last) && Time.time - last < contactCooldown) return;
        if (_pendingDamageCts.ContainsKey(t)) return;

        DelayedDamage(health, damageInfo, damageDelay).Forget();

        //  _lastHitTime[t] = Time.time;
    }

    private void CalculateShieldKnockback(IShield shield, ref DamageInfo damageInfo)
    {
        var shieldT = (shield as Component)?.transform;
        if (shieldT == null) return;

        var shieldForward = shieldT.forward;
        shieldForward.y = 0f;
        if (shieldForward.sqrMagnitude < 0.0001f)
        {
            shieldForward = (transform.position - shieldT.transform.position).normalized;
            shieldForward.y = 0f;
            Debug.Log("shieldForward was zero");
        }

        shieldForward.Normalize();

        // var side = Mathf.Sign(Vector3.Dot(transform.position - shieldT.position, shieldForward));
        // var knockbackDir = shieldForward * side;
        var knockbackDir = shieldForward;
        var force = shield.KnockbackForce;

        ApplyKnockback(knockbackDir, force);
        damageInfo.IsReflected = true;
    }

    private void PropagateKnockbackToEnemy(WalkingEnemyController otherEnemyController)
    {
        var knockbackDirection = _knockbackVelocity.normalized;
        knockbackDirection.y = 0f;
        knockbackDirection.Normalize();

        var propagatedForce = _knockbackVelocity.magnitude * knockbackPropagationForce;
        otherEnemyController.ApplyKnockback(knockbackDirection, propagatedForce);

        _knockbackVelocity *= 1f - knockbackPropagationForce * 0.2f;
    }

    public void ApplyKnockback(Vector3 direction, float force)
    {
        direction.y = 0f;
        direction.Normalize();
        _knockbackVelocity = direction * force;
    }

    private async UniTaskVoid DelayedDamage(AbstractHealth health, DamageInfo damageInfo, float delay)
    {
        var t = health.transform;
        var cts = new CancellationTokenSource();
        _pendingDamageCts[t] = cts;

        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: cts.Token);

            if (_knockbackVelocity.sqrMagnitude > minKnockbackThreshold * minKnockbackThreshold)
                return;

            if (health.Health <= 0f) return;

            DealDamage(health, damageInfo);

            _lastHitTime[t] = Time.time;
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            if (_pendingDamageCts.TryGetValue(t, out var stored) && stored == cts) _pendingDamageCts.Remove(t);
            cts.Dispose();
        }
    }

    private void OnDestroy()
    {
        foreach (var kv in _pendingDamageCts)
        {
            kv.Value.Cancel();
            kv.Value.Dispose();
        }

        _pendingDamageCts.Clear();
    }
}