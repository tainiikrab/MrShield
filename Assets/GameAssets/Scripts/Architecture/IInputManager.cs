using UnityEngine;

public interface IInputManager
{
    public Vector2 GetPointerPosition();

    public Vector2 GetNormalizedPointerPosition()
    {
        var w = Screen.width;
        var h = Screen.height;

        if (w <= 0 || h <= 0)
            return Vector2.zero;

        var pointerPosition = GetPointerPosition();

        var x = (pointerPosition.x / w - 0.5f) * 2f;
        var y = (pointerPosition.y / h - 0.5f) * 2f;

        x = Mathf.Clamp(x, -1f, 1f);
        y = Mathf.Clamp(y, -1f, 1f);

        return new Vector2(x, y);
    }

    public Vector3 GetMovementAxis();

    public bool IsFirePressed();
}