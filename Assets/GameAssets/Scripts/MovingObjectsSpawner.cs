using System.Collections.Generic;
using UnityEngine;

public class MovingObjectsSpawner : ObjectSpawner
{
    [SerializeField] private Transform[] prefabs;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float lifetime = 20f;
    [SerializeField] private int initialFillAmount = 50;
    [SerializeField] private float maxInitialLife = 100f;

    [SerializeField] private bool isPrewarmed = true;
    private Vector3 _forwardMovement;

    private struct PropData
    {
        public Transform Transform;
        public float DestroyTime;
    }

    private readonly List<PropData> _props = new();
    private readonly Queue<Transform> _pool = new();
    private bool _isInitialFill;

    protected override void Awake()
    {
        base.Awake();
        _forwardMovement = speed * transform.forward;

        InitPool(initialFillAmount);

        _isInitialFill = true;
        if (isPrewarmed)
            Spawn(initialFillAmount);
        _isInitialFill = false;
    }

    private void InitPool(int poolSize)
    {
        _pool.Clear();

        for (var i = 0; i < poolSize; i++)
        {
            var prefab = prefabs[Random.Range(0, prefabs.Length)];
            var prop = Instantiate(prefab, transform);
            prop.gameObject.SetActive(false);
            _pool.Enqueue(prop);
        }
    }

    private bool TryGetFromPool(out Transform prop)
    {
        if (_pool.Count == 0)
        {
            prop = null;
            return false;
        }

        prop = _pool.Dequeue();
        prop.gameObject.SetActive(true);
        return true;
    }

    private void ReturnToPool(Transform prop)
    {
        prop.gameObject.SetActive(false);
        prop.SetParent(transform, false);
        _pool.Enqueue(prop);
    }

    public override void Spawn(int amount)
    {
        while (amount-- > 0)
        {
            if (!TryGetFromPool(out var prop))
            {
                Debug.LogWarning("No props in pool");
                return;
            }

            var randomPos = GetRandomPos();
            prop.position = randomPos;
            prop.rotation = Quaternion.identity;

            var remainingLife = lifetime;

            if (_isInitialFill && isPrewarmed)
            {
                var age = Random.Range(0f, maxInitialLife);
                prop.position += _forwardMovement * age;
                remainingLife = lifetime - age;

                if (remainingLife <= 0f)
                {
                    ReturnToPool(prop);
                    continue;
                }
            }

            _props.Add(new PropData
            {
                Transform = prop,
                DestroyTime = Time.time + remainingLife
            });
        }
    }

    private void Update()
    {
        var now = Time.time;

        for (var i = _props.Count - 1; i >= 0; i--)
        {
            var p = _props[i];

            if (now >= p.DestroyTime)
            {
                ReturnToPool(p.Transform);
                _props.RemoveAt(i);
            }
            else
            {
                p.Transform.position += _forwardMovement * Time.deltaTime;
            }
        }
    }
}