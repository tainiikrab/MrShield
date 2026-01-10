using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class ShieldCooldownUI : MonoBehaviour
{
    [SerializeField] private ShieldController shield;
    [SerializeField] private Image cooldownImage;
    [SerializeField] private CanvasGroup canvasGroup;

    [SerializeField] private float showDuration = 0.1f;
    [SerializeField] private float hideDuration = 0.2f;

    private bool _isVisible;
    private float BumpTime => shield.NextBumpTime;

    private Tween _alphaTween;

    private void Update()
    {
        var rawFrac = 1 - Mathf.Clamp01((BumpTime - Time.time) / shield.BumpCooldown);
        var quantized = Mathf.Round(rawFrac * 100f) / 100f;

        if (Mathf.Abs(cooldownImage.fillAmount - quantized) > 0.0001f)
            cooldownImage.fillAmount = quantized;

        var shouldBeVisible = BumpTime > Time.time;
        if (shouldBeVisible != _isVisible)
            ToggleVisibility(shouldBeVisible);
    }

    private void ToggleVisibility(bool isVisible)
    {
        var targetAlpha = isVisible ? 1f : 0f;

        if (Mathf.Abs(canvasGroup.alpha - targetAlpha) < 0.0001f)
        {
            _isVisible = isVisible;
            return;
        }

        if (_alphaTween.isAlive)
            _alphaTween.Stop();

        _alphaTween = Tween.Alpha(
            canvasGroup,
            targetAlpha,
            isVisible ? showDuration : hideDuration
        );

        _isVisible = isVisible;
    }
}