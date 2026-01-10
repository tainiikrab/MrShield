using System;
using UnityEngine;

public class WorldCanvasRotator : MonoBehaviour
{
    [SerializeField] private Transform target;
    private bool _isTargetSet;

    private void Start()
    {
        if (target == null) target = Camera.main.transform;
        if (target == null) Debug.LogError("No target found");
        else _isTargetSet = true;
    }

    private void Update()
    {
        if (!_isTargetSet) return;
        transform.LookAt(target);
        transform.Rotate(0, 180, 0);
    }
}