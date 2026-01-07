using UnityEngine;

public interface IInputManager
{
    public Vector2 GetPointerPosition();

    public Vector2 GetNormalizedPointerPosition()
    {
        var pointerPosition = GetPointerPosition();
        var relativePosition = new Vector2();
        relativePosition.x = (pointerPosition.x / Screen.width - 0.5f) * 2;
        relativePosition.y = (pointerPosition.y / Screen.height - 0.5f) * 2;
        return relativePosition;
    }

    public Vector3 GetMovementAxis();
}