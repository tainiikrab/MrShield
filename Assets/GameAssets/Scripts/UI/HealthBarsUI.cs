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

        playerHealth.OnDamaged += UpdatePlayerHealthBar;
        shieldHealth.OnDamaged += UpdateShieldHealthBar;
    }

    private void UpdatePlayerHealthBar(int health)
    {
        Tween.UISliderValue(playerHealthBar, playerHealthBar.value, health, 0.2f);
        // playerHealthBar.value = health;
    }

    private void UpdateShieldHealthBar(int health)
    {
        Tween.UISliderValue(shieldHealthBar, shieldHealthBar.value, health, 0.2f);
        shieldHealthBar.value = health;
    }
}