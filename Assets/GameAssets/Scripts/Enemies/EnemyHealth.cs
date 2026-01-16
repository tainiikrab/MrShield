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

    private void AnimateDeath()
    {
        if (_hitSeq.isAlive)
            _hitSeq.Stop();

        var seq = Sequence.Create();
        seq.Chain(Tween.PositionY(transform, transform.position.y - 0.55f, 0.14f, Ease.InCubic));
        seq.Group(Tween.ShakeLocalPosition(transform, new Vector3(0.08f, 0.04f, 0f), 0.10f, 22, true));
        seq.Group(Tween.Scale(transform, new Vector3(1.35f, 0.6f, 1f), 0.08f, Ease.OutQuad));
        seq.Chain(Tween.Scale(transform, 0f, 0.16f, Ease.InBack));

        seq.ChainCallback(() => Destroy(gameObject));
    }

    private Sequence _hitSeq;

    private void AnimateHit()
    {
        if (_hitSeq.isAlive)
            _hitSeq.Stop();

        _hitSeq = Sequence.Create();

        _hitSeq.Group(Tween.PunchLocalPosition(
            animatedTransform,
            (Vector3.up * 0.05f + Vector3.right * 0.04f) * (1f + 2.4f),
            0.5f,
            18
        ));
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
}