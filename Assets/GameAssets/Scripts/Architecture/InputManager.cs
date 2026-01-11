using UnityEngine;

public class InputManager : IInputManager
{
    private const string HorizontalAxis = "Horizontal";
    private const string VerticalAxis = "Vertical";
    private const string FireButton = "Fire1";

    public Vector2 GetPointerPosition()
    {
        return Input.mousePosition;
    }

    public Vector3 GetMovementAxis()
    {
        var movement = new Vector3(Input.GetAxis(HorizontalAxis), 0, Input.GetAxis(VerticalAxis));
        movement = Vector3.ClampMagnitude(movement, 1);
        return movement;
    }

    public bool IsFirePressed()
    {
        return Input.GetButton(FireButton);
    }
}