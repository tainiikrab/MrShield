using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using Random = UnityEngine.Random;

[RequireComponent(typeof(BoxCollider))]
public class ObjectSpawner : MonoBehaviour, IHasTarget
{
    [SerializeField] protected Transform objectPrefab;
    [SerializeField] private float spawnRate = 4f;
    [SerializeField] private int spawnAmount = 4;

    private BoxCollider _spawnBox;

    protected Vector3 _spawnBoxCenter;
    protected Vector3 _spawnBoxHalfSize;

    private Vector3 _lastPos;
    private Quaternion _lastRot;
    private Vector3 _lastScale;

    private CancellationTokenSource _spawnCts;

    [Inject] public Transform Target { get; set; }

    protected virtual void Awake()
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
        while (!ct.IsCancellationRequested)
        {
            Spawn(spawnAmount);
            await UniTask.Delay(TimeSpan.FromSeconds(spawnRate), false, cancellationToken: ct);
        }
    }

    public virtual void Spawn(int amount)
    {
        while (amount-- > 0)
        {
            var randomPos = GetRandomPos();

            Instantiate(objectPrefab, randomPos, Quaternion.identity);
        }
    }

    protected Vector3 GetRandomPos()
    {
        return _spawnBoxCenter + new Vector3(Random.Range(-_spawnBoxHalfSize.x, _spawnBoxHalfSize.x),
            Random.Range(-_spawnBoxHalfSize.y, _spawnBoxHalfSize.y),
            Random.Range(-_spawnBoxHalfSize.z, _spawnBoxHalfSize.z));
    }

    private void CacheBox()
    {
        var tr = _spawnBox.transform;

        (_spawnBoxCenter, _spawnBoxHalfSize) = _spawnBox.GetWorldSpaceBounds();

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

public static class BoxColliderExtensions
{
    public static (Vector3 center, Vector3 halfSize) GetWorldSpaceBounds(this BoxCollider boxCollider)
    {
        var transform = boxCollider.transform;
        var center = transform.TransformPoint(boxCollider.center);
        var halfSize = Vector3.Scale(boxCollider.size * 0.5f, transform.lossyScale);
        return (center, halfSize);
    }
}