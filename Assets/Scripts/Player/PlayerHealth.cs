using System;
using UnityEngine;

public interface IDamageable {
    void TakeDamage(float damage, Vector2 attackDirection = default);
}

public class PlayerHealth : MonoBehaviour, IDamageable {

    public event EventHandler OnHealthChanged;
    [SerializeField] private bool knockbackAfterTakingDamage;
    [SerializeField] private float knockbackForce;

    private Rigidbody2D rb;
    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
    }

    public void TakeDamage(float damage, Vector2 attackDirection = default) {
        Player.Instance.playerStats.playerCurrentStats.TakeDamage(damage);

        if (knockbackAfterTakingDamage) {
            // rinculo player
            Player.Instance.playerMovement.ApplyKnockback(attackDirection, knockbackForce);
        }
        // flash colore sprite player

        OnHealthChanged?.Invoke(this, EventArgs.Empty);

        // check:
        // se currentHealth <= 0 -> schermata gameOver -> ritorno in hub
    }

    public void Heal(float amount) {
        Player.Instance.playerStats.playerCurrentStats.Heal(amount);

        OnHealthChanged?.Invoke(this, EventArgs.Empty);
    }
}
