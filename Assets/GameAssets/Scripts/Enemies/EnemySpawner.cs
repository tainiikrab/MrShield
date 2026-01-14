using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using Random = UnityEngine.Random;

[RequireComponent(typeof(BoxCollider))]
public class EnemySpawner : MonoBehaviour, IHasTarget
{
    [SerializeField] private Transform enemyPrefab;
    [SerializeField] private float spawnRate = 4f;
    [SerializeField] private int spawnAmount = 4;

    private BoxCollider _spawnBox;

    private Vector3 _worldCenter;
    private Vector3 _halfSize;

    private Vector3 _lastPos;
    private Quaternion _lastRot;
    private Vector3 _lastScale;

    private CancellationTokenSource _spawnCts;

    [Inject] public Transform Target { get; set; }

    private void Awake()
    {
        _spawnBox = GetComponent<BoxCollider>();
        CacheBox();
    }

    private void OnEnable()
    {
        _spawnBox.enabled = false;

        _spawnCts?.Cancel();
        _spawnCts?.Dispose();
        _spawnCts = new CancellationTokenSource();

        StartSpawning(_spawnCts.Token).Forget(ex =>
        {
            if (ex is OperationCanceledException) return;
            Debug.LogException(ex);
        });
    }

    private void OnDisable()
    {
        _spawnCts?.Cancel();
        _spawnCts?.Dispose();
        _spawnCts = null;
    }

    private async UniTask StartSpawning(CancellationToken ct)
    {
        while (true)
        {
            Spawn(spawnAmount);
            await UniTask.Delay(TimeSpan.FromSeconds(spawnRate), false, cancellationToken: ct);
        }
    }

    public void Spawn(int amount)
    {
        while (amount-- > 0)
        {
            var random = _worldCenter + new Vector3(
                Random.Range(-_halfSize.x, _halfSize.x),
                Random.Range(-_halfSize.y, _halfSize.y),
                Random.Range(-_halfSize.z, _halfSize.z)
            );

            var enemy = Instantiate(enemyPrefab, random, Quaternion.identity);
            if (enemy.TryGetComponent<IHasTarget>(out var component)) component.Target = Target;
            else throw new MissingComponentException($"{enemy.name} must implement {nameof(IHasTarget)}");
        }
    }

    private void CacheBox()
    {
        var tr = _spawnBox.transform;

        _worldCenter = tr.TransformPoint(_spawnBox.center);
        _halfSize = Vector3.Scale(_spawnBox.size * 0.5f, tr.lossyScale);

        _lastPos = tr.position;
        _lastRot = tr.rotation;
        _lastScale = tr.lossyScale;
    }

    private void UpdateCacheIfTransformChanged()
    {
        var tr = _spawnBox.transform;

        if (tr.position != _lastPos || tr.rotation != _lastRot || tr.lossyScale != _lastScale)
            CacheBox();
    }
}