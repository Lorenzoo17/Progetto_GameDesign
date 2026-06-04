using System;
using UnityEngine;

public class DamageEventArgs : EventArgs
{
    public float Damage { get; }
    public Vector2 AttackDirection { get; }

    public DamageEventArgs(float damage, Vector2 attackDirection)
    {
        Damage = damage;
        AttackDirection = attackDirection;
    }
}
public class HealthSystem : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth;
    public float CurrentHealth { get; private set; }
    public bool resetsSceneOnDeath = false;

    public LevelLoader levelLoader;

    [SerializeField] private float defense;
    public event EventHandler<DamageEventArgs> OnDamageTaken;

    private DamageEffectVisuals damageEffectVisuals;

    private void Awake()
    {
        CurrentHealth = maxHealth;
        defense = 0;
        damageEffectVisuals = GetComponent<DamageEffectVisuals>();
    }

    public float GetHealthPercentage()
    {
        return (CurrentHealth / maxHealth) * 100f;
    }

    public void TakeDamage(DamageInfo damageInfo)
    {
        DamageInfo modifiedDamageInfo = TakeDamageLol(damageInfo);
        CurrentHealth -= modifiedDamageInfo.Damage[DamageType.Physical];
        if (damageInfo.Damage[DamageType.Poison] > 0)
        {
            TakePoisonDamage(damageInfo);
        }
        OnDamageTaken?.Invoke(this, new DamageEventArgs(modifiedDamageInfo.Damage[DamageType.Physical], modifiedDamageInfo.Direction));

        // Trigga visual effects
        HandleDamageVisuals(modifiedDamageInfo);

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound2D(SoundID.EnemyHit, .25f);
        }

        if (CurrentHealth <= 0)
        {
            Destroy(gameObject);
            if (resetsSceneOnDeath)
            {
                levelLoader.LoadNextScene("CombatHub");
            }
        }
    }

    public void TakePoisonDamage(DamageInfo damageInfo)
    {
        float poisonDamage = damageInfo.Damage[DamageType.Poison];
        CurrentHealth -= poisonDamage;
        OnDamageTaken?.Invoke(this, new DamageEventArgs(poisonDamage, damageInfo.Direction));

        // Trigga visual effects per veleno
        if (damageEffectVisuals != null && poisonDamage > 0)
        {
            damageEffectVisuals.PlayPoisonEffect();
        }

        // 🔥 SISTEMA DI VELENO CUMULATIVO
        if (damageInfo.AppliedEffects.Contains("PoisonApplied") && poisonDamage > 0)
        {
            // Cerca se esiste già un effetto veleno
            PoisonEffect poisonEffect = GetComponent<PoisonEffect>();
            Debug.Log($"Applying poison: {poisonDamage} damage. Current health: {CurrentHealth}");
            if (poisonEffect == null)
            {
                // Primo colpo di veleno: crea il componente
                poisonEffect = gameObject.AddComponent<PoisonEffect>();
            }

            // Aggiungi il veleno (accumula con quelli precedenti)
            poisonEffect.AddPoison(damageInfo.Source, poisonDamage);
        }

        if (CurrentHealth <= 0)
        {
            Destroy(gameObject);
            if (resetsSceneOnDeath)
            {
                levelLoader.LoadNextScene("CombatHub");
            }
        }
    }

    private void HandleDamageVisuals(DamageInfo damageInfo)
    {
        if (damageEffectVisuals == null) return;

        // Particella rossa se il danno è stato boosted
        if (damageInfo.AppliedEffects.Contains("DamageBoost"))
        {
            damageEffectVisuals.PlayDamageBoostEffect();
        }

        // Maschera verde se è stato applicato veleno
        if (damageInfo.AppliedEffects.Contains("PoisonApplied"))
        {
            damageEffectVisuals.PlayPoisonEffect();
        }
    }

    private DamageInfo TakeDamageLol(DamageInfo damageInfo)
    {
        float defensePerc = 100f / (100f + defense);

        DamageInfo modifiedDamageInfo = new DamageInfo(
            damageInfo.Damage[DamageType.Physical] * defensePerc,
            damageInfo.Direction,
            damageInfo.Source,
            damageInfo.SourceFaction
        );

        // Copia gli effetti applicati
        modifiedDamageInfo.AppliedEffects = damageInfo.AppliedEffects;

        return modifiedDamageInfo;
    }

    private DamageInfo TakeDamageOther(DamageInfo damageInfo)
    {
        float defensePerc = (100f / (100f + defense));
        float damage = damageInfo.Damage[DamageType.Physical];

        DamageInfo modifiedDamageInfo = new DamageInfo(
            MathF.Log(damage, 2) * defensePerc * MathF.Sqrt(damage),
            damageInfo.Direction,
            damageInfo.Source,
            damageInfo.SourceFaction
        );

        modifiedDamageInfo.AppliedEffects = damageInfo.AppliedEffects;

        return modifiedDamageInfo;
    }
}
