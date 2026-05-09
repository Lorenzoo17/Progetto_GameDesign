using System;
using UnityEngine;

public class DamageEventArgs : EventArgs {
    public float Damage { get; }
    public Vector2 AttackDirection { get; }

    public DamageEventArgs(float damage, Vector2 attackDirection) {
        Damage = damage;
        AttackDirection = attackDirection;
    }
}
public class HealthSystem : MonoBehaviour, IDamageable {

    [SerializeField] private float maxHealth;
    public float CurrentHealth { get; private set; }

    public event EventHandler<DamageEventArgs> OnDamageTaken;

    private void Awake() {
        CurrentHealth = maxHealth;
    }

    public void TakeDamage(DamageInfo damageInfo) {
        CurrentHealth -= damageInfo.Damage;
        OnDamageTaken?.Invoke(this, new DamageEventArgs(damageInfo.Damage, damageInfo.Direction));

        if (SoundManager.Instance != null) {
            SoundManager.Instance.PlaySound2D(SoundID.EnemyHit, .25f);
        }

        if (CurrentHealth <= 0) {
            Destroy(gameObject);
        }
    }
}
