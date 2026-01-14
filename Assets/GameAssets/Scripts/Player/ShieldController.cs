using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using PrimeTween;

public class ShieldController : MonoBehaviour, IShield
{
    [Inject] private IInputManager _inputManager;

    [Header("References")] [SerializeField]
    private ShieldAnimatorConfig shieldAnimatorConfig;

    [SerializeField] private Transform shieldMesh;

    [Space(5)] [Header("Shield Settings")] [SerializeField]
    private float knockbackForce;

    [SerializeField] private float bumpCooldown = 0.5f;
    [SerializeField] private float bumpDuration = 0.2f;

    private Transform _transform;
    private ShieldAnimator _shieldAnimator;
    private AbstractHealth _health;
    public bool IsReflecting { get; private set; } = false;
    public float KnockbackForce => knockbackForce;
    public float BumpCooldown => bumpCooldown;
    public float NextBumpTime { get; private set; }
    private bool _isBumping = false;

    private void Awake()
    {
        _transform = transform;
        _shieldAnimator = new ShieldAnimator(shieldMesh, shieldAnimatorConfig);
    }

    private void Update()
    {
        var y = _inputManager.GetNormalizedPointerPosition().x * 90f;
        var x = Mathf.Min(0, -_inputManager.GetNormalizedPointerPosition().y) * 45f;
        _transform.rotation = Quaternion.Euler(x, y, 0);

        if (_inputManager.IsFirePressed())
            _ = BumpShield();
    }


    private async UniTask BumpShield()
    {
        if (_isBumping) return;
        if (Time.time < NextBumpTime) return;

        NextBumpTime = Time.time + bumpCooldown;

        IsReflecting = true;
        _isBumping = true;

        await _shieldAnimator.BumpAsync(bumpDuration);

        IsReflecting = false;
        _isBumping = false;
    }
}

public class ShieldAnimator
{
    private Transform _shieldMesh;
    private Sequence _bumpSequence;
    private float _meshInitialZ;
    private ShieldAnimatorConfig _cfg;

    public ShieldAnimator(Transform mesh, ShieldAnimatorConfig cfg)
    {
        _shieldMesh = mesh;
        _meshInitialZ = _shieldMesh.localPosition.z;
        _cfg = cfg;
    }

    public async UniTask BumpAsync(float t)
    {
        if (_bumpSequence.isAlive)
            _bumpSequence.Stop();

        var growthWeight = Mathf.Max(0f, _cfg.bumpGrowthTime);
        var holdWeight = Mathf.Max(0f, _cfg.bumpHoldTime);
        var shrinkWeight = Mathf.Max(0f, _cfg.bumpShrinkTime);

        var totalWeight = growthWeight + holdWeight + shrinkWeight;
        if (totalWeight <= 0f)
        {
            Debug.LogError("Invalid shield animator config");
            totalWeight = 1f;
        }

        var growthT = t * (growthWeight / totalWeight);
        var holdT = t * (holdWeight / totalWeight);
        var shrinkT = t * (shrinkWeight / totalWeight);

        _bumpSequence = Sequence.Create()
            .Chain(Tween.LocalPositionZ(_shieldMesh, _meshInitialZ + _cfg.bumpZOffset, growthT, _cfg.bumpEasing))
            .Group(Tween.Scale(_shieldMesh, Vector3.one * _cfg.bumpScale, growthT, _cfg.bumpEasing))
            .ChainDelay(holdT)
            .Chain(Tween.Scale(_shieldMesh, Vector3.one, shrinkT, _cfg.shrinkEasing))
            .Group(Tween.LocalPositionZ(_shieldMesh, _meshInitialZ, shrinkT, _cfg.shrinkEasing));

        await _bumpSequence;
    }
}

[Serializable]
public class ShieldAnimatorConfig
{
    [Min(0f)] public float bumpScale = 1.2f;
    [Min(0f)] public float bumpZOffset = 0.1f;

    // relative weights for bump animation
    [Min(0f)] public float bumpGrowthTime = 0.3f;
    [Min(0f)] public float bumpHoldTime = 0.1f;
    [Min(0f)] public float bumpShrinkTime = 0.6f;

    public Ease bumpEasing = Ease.Default;
    public Ease shrinkEasing = Ease.Default;
}