using UnityEngine;
using UnityEngine.UI;
using PrimeTween;

public class HealthBarsUI : MonoBehaviour
{
    [SerializeField] private AbstractHealth playerHealth;
    [SerializeField] private AbstractHealth shieldHealth;

    [SerializeField] private Slider playerHealthBar;
    [SerializeField] private Slider shieldHealthBar;

    private void Start()
    {
        playerHealthBar.maxValue = playerHealth.MaxHealth;
        shieldHealthBar.maxValue = shieldHealth.MaxHealth;
        playerHealthBar.value = playerHealth.Health;
        shieldHealthBar.value = shieldHealth.Health;

        playerHealth.OnHealthChanged += UpdatePlayerHealthBar;
        shieldHealth.OnHealthChanged += UpdateShieldHealthBar;
    }

    private void UpdatePlayerHealthBar(float health)
    {
        Tween.UISliderValue(playerHealthBar, playerHealthBar.value, health, 0.2f);
        // playerHealthBar.value = health;
    }

    private void UpdateShieldHealthBar(float health)
    {
        Tween.UISliderValue(shieldHealthBar, shieldHealthBar.value, health, 0.2f);
        shieldHealthBar.value = health;
    }
}