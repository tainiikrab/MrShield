using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using PrimeTween;

public class ShieldController : MonoBehaviour, IProjectileReflector
{
    [Inject] private IInputManager _inputManager;

    private Transform _transform;
    private ShieldAnimator _shieldAnimator;

    public bool IsReflecting { get; private set; } = false;

    [SerializeField] private ShieldAnimatorConfig shieldAnimatorConfig;
    [SerializeField] private Transform shieldMesh;

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

        if (_inputManager.IsFirePressed()) BumpShield();
    }


    private async void BumpShield()
    {
        IsReflecting = true;
        await _shieldAnimator.BumpAsync();
        IsReflecting = false;
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

    public async UniTask BumpAsync()
    {
        if (_bumpSequence.isAlive)
            _bumpSequence.Stop();

        _bumpSequence = Sequence.Create()
            .Chain(Tween.LocalPositionZ(_shieldMesh, _meshInitialZ + _cfg.bumpZOffset, _cfg.bumpGrowthTime))
            .Group(Tween.Scale(_shieldMesh, Vector3.one * _cfg.bumpScale, _cfg.bumpGrowthTime))
            .Chain(Tween.Scale(_shieldMesh, Vector3.one, _cfg.bumpShrinkTime))
            .Group(Tween.LocalPositionZ(_shieldMesh, _meshInitialZ, _cfg.bumpShrinkTime));

        await _bumpSequence;
    }
}

[Serializable]
public class ShieldAnimatorConfig
{
    public float bumpScale = 1.2f;
    public float bumpGrowthTime = 0.1f;
    public float bumpShrinkTime = 0.2f;
    public float bumpZOffset = 0.1f;
}