using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementIllusionPropsSpawner : ObjectSpawner
{
    [SerializeField] private Transform[] prefabs;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float lifetime = 20f;
    [SerializeField] private int initialFillAmount = 50;
    [SerializeField] private float maxInitialLife = 100;

    private Vector3 _forwardMovement;

    private struct PropData
    {
        public Transform Transform;
        public float DestroyTime;
    }

    private readonly List<PropData> _props = new();
    private bool _isInitialFill;

    protected override void Awake()
    {
        base.Awake();
        _forwardMovement = speed * transform.forward;

        _isInitialFill = true;
        Spawn(initialFillAmount);
        _isInitialFill = false;
    }

    public override void Spawn(int amount)
    {
        while (amount-- > 0)
        {
            var randomPos = GetRandomPos();
            var propPrefab = prefabs[Random.Range(0, prefabs.Length)];
            var prop = Instantiate(propPrefab, randomPos, Quaternion.identity, transform);

            var remainingLife = lifetime;

            if (_isInitialFill)
            {
                var age = Random.Range(0, maxInitialLife);
                prop.position += _forwardMovement * age;
                remainingLife = lifetime - age;
                if (remainingLife <= 0f)
                {
                    Destroy(prop.gameObject);
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

            if (p.Transform == null || now >= p.DestroyTime)
            {
                if (p.Transform != null)
                    Destroy(p.Transform.gameObject);

                _props.RemoveAt(i);
            }
            else
            {
                p.Transform.position += _forwardMovement * Time.deltaTime;
            }
        }
    }
}