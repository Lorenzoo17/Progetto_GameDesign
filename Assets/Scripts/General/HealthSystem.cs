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

    [SerializeField] private float defense;
    public event EventHandler<DamageEventArgs> OnDamageTaken;

    private void Awake() {
        CurrentHealth = maxHealth;
        defense = 0;
    }

    public void TakeDamage(DamageInfo damageInfo) {
        DamageInfo modifiedDamageInfo = TakeDamageLol(damageInfo);
        CurrentHealth -= modifiedDamageInfo.Damage;
        OnDamageTaken?.Invoke(this, new DamageEventArgs(modifiedDamageInfo.Damage, modifiedDamageInfo.Direction));

        if (CurrentHealth <= 0) {
            Destroy(gameObject);
        }
    }

    private DamageInfo TakeDamageLol(DamageInfo damageInfo) {
        DamageInfo modifiedDamageInfo = new DamageInfo(
            damageInfo.Damage * (100/(100 + defense)), // formula danno ridotto da armor
            damageInfo.Direction,
            damageInfo.Source,
            damageInfo.SourceFaction
        );
        return modifiedDamageInfo;
    }
    private DamageInfo TakeDamageOther(DamageInfo damageInfo) {
        DamageInfo modifiedDamageInfo = new DamageInfo(
            Math.Log2(damageInfo.Damage) * (100/(100 + defense)), // formula danno ridotto da armor
            damageInfo.Direction,
            damageInfo.Source,
            damageInfo.SourceFaction
        );
        return damageInfo;
    }
}
