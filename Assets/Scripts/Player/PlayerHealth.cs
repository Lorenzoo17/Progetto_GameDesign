using FirstGearGames.SmoothCameraShaker;
using System;
using System.Collections;
using UnityEngine;

public interface IDamageable {
    void TakeDamage(float damage, Vector2 attackDirection = default);
}

public class PlayerHealth : MonoBehaviour, IDamageable {

    public event EventHandler OnHealthChanged;
    [SerializeField] private bool knockbackAfterTakingDamage;
    [SerializeField] private float knockbackForce;

    private Color initialColor; // usato per blink dopo take damage

    private Rigidbody2D rb;
    private SpriteRenderer sr;

    [SerializeField] private HealthBar healthBar;

    [SerializeField] private float invincibilityTime = 1;
    [SerializeField] private float blinkAfterDamageRate = 0.2f;
    private int blinkAmount;
    private bool invincible = false;
    private Coroutine blinkCoroutine;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        blinkAmount = (int)(invincibilityTime / (blinkAfterDamageRate * 2));

        initialColor = sr.color;
    }

    private void Update() {
        // sr.color = Color.Lerp(sr.color, initialColor, blinkAfterDamageTime);
    }

    public void TakeDamage(float damage, Vector2 attackDirection = default) {
        if (Player.Instance.playerMovement.IsDodging() || invincible) return; // mentre sta schivando non prende danno o mentre e' invincibile (perche appena stato colpito)
          
        Player.Instance.playerStats.playerCurrentStats.TakeDamage(damage);

        if (knockbackAfterTakingDamage) {
            // rinculo player
            Player.Instance.playerMovement.ApplyKnockback(attackDirection, knockbackForce);
        }
        // flash colore sprite player
        invincible = true; // diventa invincibile
        // per sicurezza per evitare coroutine stacking
        if(blinkCoroutine != null) {
            StopCoroutine(blinkCoroutine);
        }
        blinkCoroutine = StartCoroutine(DamageBlink());

        // screen shake
        CameraShakerHandler.Shake(Player.Instance.cameraShakeData);

        OnHealthChanged?.Invoke(this, EventArgs.Empty);

        // check:
        // se currentHealth <= 0 -> schermata gameOver -> ritorno in hub
    }

    private IEnumerator DamageBlink() {
        for(int i = 0; i < blinkAmount; i++) {
            sr.color = Color.white * 3f;

            yield return new WaitForSeconds(blinkAfterDamageRate);

            sr.color = initialColor;

            yield return new WaitForSeconds(blinkAfterDamageRate);
        }
        sr.color = initialColor; // per sicurezza
        invincible = false;
    }

    public void Heal(float amount) {
        Player.Instance.playerStats.playerCurrentStats.Heal(amount);

        OnHealthChanged?.Invoke(this, EventArgs.Empty);
    }

    public float GetHealthPercentage() {
        return Player.Instance.playerStats.playerCurrentStats.GetCurrentHealth() / Player.Instance.playerStats.playerCurrentStats.GetMaxHealth();
    }
}
