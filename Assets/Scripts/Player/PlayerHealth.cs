using System;
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
    [SerializeField] private float blinkAfterDamageTime = 2f;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        initialColor = sr.color;
    }

    private void Update() {
        sr.color = Color.Lerp(sr.color, initialColor, blinkAfterDamageTime);
    }

    public void TakeDamage(float damage, Vector2 attackDirection = default) {
        Player.Instance.playerStats.playerCurrentStats.TakeDamage(damage);

        if (knockbackAfterTakingDamage) {
            // rinculo player
            Player.Instance.playerMovement.ApplyKnockback(attackDirection, knockbackForce);
        }
        // flash colore sprite player
        sr.color = Color.white * 3f;

        OnHealthChanged?.Invoke(this, EventArgs.Empty);

        // check:
        // se currentHealth <= 0 -> schermata gameOver -> ritorno in hub
    }

    public void Heal(float amount) {
        Player.Instance.playerStats.playerCurrentStats.Heal(amount);

        OnHealthChanged?.Invoke(this, EventArgs.Empty);
    }
}
