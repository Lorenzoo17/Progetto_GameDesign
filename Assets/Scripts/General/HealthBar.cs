using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour {

    [SerializeField] private Slider healthBarSlider;

    private void Start() {
        if (Player.Instance == null) return;

        Player.Instance.playerHealth.OnHealthChanged += PlayerHealth_OnHealthChanged;
    }

    private void PlayerHealth_OnHealthChanged(object sender, System.EventArgs e) {
        SetHealth(Player.Instance.playerHealth.GetHealthPercentage());
    }

    public void SetHealth(float healthValue) {
        healthBarSlider.value = healthValue;
    }
}
