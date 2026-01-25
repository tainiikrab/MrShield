using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementIllusionPropsSpawner : ObjectSpawner
{
    [SerializeField] private Transform[] prefabs;

    [SerializeField] private float speed = 5f;
    [SerializeField] private float lifetime = 20f;

    private Vector3 _forwardMovement;

    private struct PropData
    {
        public Transform Transform;
        public float DestroyTime;
    }

    private readonly List<PropData> _props = new();
    private readonly List<int> _toRemove = new();

    protected override void Awake()
    {
        base.Awake();
        _forwardMovement = speed * transform.forward;
    }

    public override void Spawn(int amount)
    {
        while (amount-- > 0)
        {
            var randomPos = GetRandomPos();
            var propPrefab = prefabs[Random.Range(0, prefabs.Length)];

            var prop = Instantiate(propPrefab, randomPos, Quaternion.identity);
            _props.Add(new PropData
            {
                Transform = prop,
                DestroyTime = Time.time + lifetime
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