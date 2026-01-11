using UnityEngine;
using VContainer;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Inject] private IInputManager _inputManager;

    private CharacterController _characterController;
    [SerializeField] private float speed = 10f;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        var movement = _inputManager.GetMovementAxis();
        _characterController.SimpleMove(movement * speed);
    }
}