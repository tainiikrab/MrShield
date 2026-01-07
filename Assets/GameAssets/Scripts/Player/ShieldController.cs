using UnityEngine;
using VContainer;

public class ShieldController : MonoBehaviour
{
    [Inject] private IInputManager _inputManager;

    private Transform _transform;

    private void Awake()
    {
        _transform = transform;
    }

    private void Start()
    {
    }

    private void Update()
    {
        var y = _inputManager.GetNormalizedPointerPosition().x * 90f;
        var x = Mathf.Min(0, -_inputManager.GetNormalizedPointerPosition().y) * 45f;
        _transform.rotation = Quaternion.Euler(x, y, 0);
    }
}