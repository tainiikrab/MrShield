using System.Collections;
using PrimeTween;
using Unity.VisualScripting;
using UnityEngine;
using Sequence = PrimeTween.Sequence;

public class EnemyHealth : AbstractHealth
{
    [SerializeField] private float deathDuration = 0.20f;
    [SerializeField] private Transform animatedTransform;

    protected override void Awake()
    {
        base.Awake();
        if (animatedTransform == null) animatedTransform = transform;
    }

    protected override void HandleDeath()
    {
        base.HandleDeath();
        AnimateDeath();
    }

    private Sequence _deathSeq;

    private void AnimateDeath()
    {
        if (_hitSeq.isAlive)
            _hitSeq.Stop();

        _deathSeq = Sequence.Create();
        _deathSeq.Chain(Tween.PositionY(transform, transform.position.y - 0.55f, 0.14f, Ease.InCubic));
        _deathSeq.Group(Tween.ShakeLocalPosition(transform, new Vector3(0.08f, 0.04f, 0f), 0.10f, 22, true));
        _deathSeq.Group(Tween.Scale(transform, new Vector3(1.35f, 0.6f, 1f), 0.08f, Ease.OutQuad));
        _deathSeq.Chain(Tween.Scale(transform, 0f, 0.16f, Ease.InBack));

        _deathSeq.OnComplete(() => StartCoroutine(DestroyAtTheEndOfFrame()));
    }

    private Sequence _hitSeq;

    [SerializeField] private Vector3 hitScale = new(1.1f, 0.7f, 1.1f);

    private void AnimateHit()
    {
        if (_hitSeq.isAlive)
            _hitSeq.Stop();

        _hitSeq = Sequence.Create();

        _hitSeq.Group(Tween.Scale(transform, 0.6f, 0.1f));
        _hitSeq.Chain(Tween.Scale(transform, 1f, 0.1f));
    }

    public override void CalculateDamage(in DamageInfo inDamageInfo)
    {
        ApplyDamage(inDamageInfo.Value);
        AnimateHit();
    }

    private static readonly WaitForEndOfFrame waitEndOfFrame = new();

    private IEnumerator DestroyAtTheEndOfFrame()
    {
        yield return waitEndOfFrame;
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (_deathSeq.isAlive)
            _deathSeq.Stop();
        if (_hitSeq.isAlive)
            _hitSeq.Stop();
    }
}