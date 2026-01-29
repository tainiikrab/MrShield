using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MovingObjectsSpawner : ObjectSpawner
{
    [SerializeField] private Transform[] prefabs;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float lifetime = 20f;
    [SerializeField] private int initialFillAmount = 50;
    [SerializeField] private float maxInitialLife = 100f;

    [SerializeField] private bool isPrewarmed = true;
    [SerializeField] private float fadeDuration = 1f;
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
        if (!_isInitialFill || !isPrewarmed)
            FadeInAsync(prop, fadeDuration).Forget();
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

    private async UniTaskVoid FadeInAsync(Transform prop, float duration)
    {
        var renderers = prop.GetComponentsInChildren<Renderer>(true);
        var n = renderers.Length;
        if (n == 0 || duration <= 0f)
        {
            foreach (var r0 in renderers)
            {
                var mpb0 = new MaterialPropertyBlock();
                if (TryGetColorPropName(r0, out var pname, out var baseColor))
                {
                    baseColor.a = 1f;
                    mpb0.SetColor(pname, baseColor);
                    r0.SetPropertyBlock(mpb0);
                }
            }

            return;
        }
        var origColors = new Color[n];
        var propNames = new string[n];
        var blocks = new MaterialPropertyBlock[n];
        for (var i = 0; i < n; i++)
        {
            var r = renderers[i];
            if (TryGetColorPropName(r, out var pname, out var baseColor))
            {
                propNames[i] = pname;
                origColors[i] = baseColor;
            }
            else
            {
                propNames[i] = "_Color";
                origColors[i] = Color.white;
            }

            blocks[i] = new MaterialPropertyBlock();
            var c0 = origColors[i];
            c0.a = 0f;
            blocks[i].SetColor(propNames[i], c0);
            r.SetPropertyBlock(blocks[i]);
        }

        var t = 0f;
        while (t < duration)
        {
            await UniTask.Yield(PlayerLoopTiming.Update);
            t += Time.deltaTime;
            var a = Mathf.Clamp01(t / duration);

            for (var i = 0; i < n; i++)
            {
                var c = origColors[i];
                c.a = a;
                blocks[i].SetColor(propNames[i], c);
                if (renderers[i] != null)
                    renderers[i].SetPropertyBlock(blocks[i]);
            }
        }
        
        for (var i = 0; i < n; i++)
        {
            var c = origColors[i];
            c.a = 1f;
            blocks[i].SetColor(propNames[i], c);
            renderers[i].SetPropertyBlock(blocks[i]);
        }
    }
    
    private static bool TryGetColorPropName(Renderer r, out string propName, out Color baseColor)
    {
        propName = null;
        baseColor = Color.white;
        if (r == null) return false;

        var mat = r.sharedMaterial;
        if (mat == null) return false;

        if (mat.HasProperty("_BaseColor"))
        {
            propName = "_BaseColor";
            baseColor = mat.GetColor("_BaseColor");
            return true;
        }

        if (mat.HasProperty("_Color"))
        {
            propName = "_Color";
            baseColor = mat.GetColor("_Color");
            return true;
        }

        return false;
    }
}