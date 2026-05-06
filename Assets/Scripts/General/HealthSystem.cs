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

    public void TakePoisonDamage(DamageInfo damageInfo)
    {
        CurrentHealth -= damageInfo.Damage;
        OnDamageTaken?.Invoke(this, new DamageEventArgs(damageInfo.Damage, damageInfo.Direction));

        if (CurrentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }

    private DamageInfo TakeDamageLol(DamageInfo damageInfo) {
        float defensePerc = (100f / (100f + defense));

        DamageInfo modifiedDamageInfo = new DamageInfo(
            damageInfo.Damage * defensePerc,
            damageInfo.Direction,
            damageInfo.Source,
            damageInfo.SourceFaction
        );
        return modifiedDamageInfo;
    }
    private DamageInfo TakeDamageOther(DamageInfo damageInfo)
    {
        float defensePerc = (100f / (100f + defense));
        float damage = damageInfo.Damage;

       
        DamageInfo modifiedDamageInfo = new DamageInfo(
            MathF.Log(damage, 2) * defensePerc * MathF.Sqrt(damage),
            damageInfo.Direction,
            damageInfo.Source,
            damageInfo.SourceFaction
        );

        return modifiedDamageInfo;
    }
}
