using FirstGearGames.SmoothCameraShaker;
using System;
using UnityEngine;

public class DamageEventArgs : EventArgs {
    public float Damage { get; }
    public Vector2 AttackDirection { get; }
    public float KnockBackStrenght { get; }
    public DamageType AttackDamageType { get; }

    // di base damageType e' fisico
    public DamageEventArgs(float damage, Vector2 attackDirection, float knockBackStrenght = 0f, DamageType damageType = DamageType.Physical) {
        Damage = damage;
        AttackDirection = attackDirection;
        KnockBackStrenght = knockBackStrenght;
        AttackDamageType = damageType;
    }
}
public class BossDeathEventArgs : EventArgs {
    public string BossName { get; }

    public BossDeathEventArgs(string bossName) {
        BossName = bossName;
    }
}
public class HealthSystem : MonoBehaviour, IDamageable {
    [SerializeField] private float maxHealth;
    public float CurrentHealth { get; private set; }
    public bool isBoss = false;

    public LevelLoader levelLoader;

    [SerializeField] private float defense;
    public event EventHandler<DamageEventArgs> OnDamageTaken;
    public event EventHandler<BossDeathEventArgs> OnBossDeath;

    private DamageEffectVisuals damageEffectVisuals;

    private void Awake() {
        CurrentHealth = maxHealth;
        defense = 0;
        damageEffectVisuals = GetComponent<DamageEffectVisuals>();
    }

    public float GetHealthPercentage() {
        return (CurrentHealth / maxHealth) * 100f;
    }

    public void TakeDamage(DamageInfo damageInfo) {
        // screen shake quanto entita' prende danno (tolto da playerAttack)
        if(EffectManager.Instance != null) {
            CameraShakerHandler.Shake(EffectManager.Instance.GetShakeDataByType(ShakeDataType.MeleeAttack));
        }
        // Applica danno fisico
        ApplyPhysicalDamage(damageInfo);

        // Applica danno veleno
        if (damageInfo.Damage[DamageType.Poison] > 0) {
            TakePoisonDamage(damageInfo);
        }
    }

    private void ApplyPhysicalDamage(DamageInfo damageInfo) {
        DamageInfo modifiedDamageInfo = TakeDamageLol(damageInfo);
        CurrentHealth -= modifiedDamageInfo.Damage[DamageType.Physical];
        OnDamageTaken?.Invoke(this, new DamageEventArgs(modifiedDamageInfo.Damage[DamageType.Physical], modifiedDamageInfo.Direction, modifiedDamageInfo.KnockBackStrenght, DamageType.Physical));
        HandleDamageVisuals(modifiedDamageInfo);

        if (SoundManager.Instance != null) {
            SoundManager.Instance.PlaySound2D(SoundID.EnemyHit, .25f);
        }
        HandleDeathIfNeeded();
    }

    private void HandleDeathIfNeeded() {
        if (CurrentHealth <= 0) {
            Debug.Log($"[HealthSystem] Enemy died!");
            Destroy(gameObject);
            if (isBoss) {
                FindFirstObjectByType<PerkController>()?.ClearAllNegativePerks();
            }
        }
    }

    public void TakePoisonDamage(DamageInfo damageInfo) {
        float poisonDamage = damageInfo.Damage[DamageType.Poison];

        Debug.Log($"[HealthSystem] Taking poison damage: {poisonDamage} from {damageInfo.Source.name}");

        if (poisonDamage <= 0) return; // Non applicare se danno è 0
        CurrentHealth -= poisonDamage;
        OnDamageTaken?.Invoke(this, new DamageEventArgs(poisonDamage, damageInfo.Direction, damageType: DamageType.Poison));

        Debug.Log($"[HealthSystem] Taking poison damage: {poisonDamage}. Current health: {CurrentHealth}. Effect: {string.Join(", ", damageInfo.AppliedEffects)}");

        // Trigga visual effects per veleno
        if (damageEffectVisuals != null && poisonDamage > 0) {
            damageEffectVisuals.PlayPoisonEffect();
        }

        // 🔥 SISTEMA DI VELENO CUMULATIVO
        // Solo se è il primo colpo di veleno (contiene "PoisonApplied")
        if (damageInfo.AppliedEffects.Contains("PoisonApplied") && poisonDamage > 0) {
            Debug.Log($"[HealthSystem] PoisonApplied detected! Creating/updating PoisonEffect");

            // Cerca se esiste già un effetto veleno
            PoisonEffect poisonEffect = GetComponent<PoisonEffect>();

            if (poisonEffect == null) {
                Debug.Log($"[HealthSystem] Creating new PoisonEffect component");
                poisonEffect = gameObject.AddComponent<PoisonEffect>();
            }

            // Aggiungi il veleno (accumula con quelli precedenti)
            poisonEffect.AddPoison(damageInfo.Source, poisonDamage);
        }
        HandleDeathIfNeeded();
    }

    private void HandleDamageVisuals(DamageInfo damageInfo) {
        if (damageEffectVisuals == null) return;

        // Particella rossa se il danno è stato boosted
        if (damageInfo.AppliedEffects.Contains("DamageBoost")) {
            damageEffectVisuals.PlayDamageBoostEffect();
        }

        // Maschera verde se è stato applicato veleno
        if (damageInfo.AppliedEffects.Contains("PoisonApplied")) {
            damageEffectVisuals.PlayPoisonEffect();
        }
    }

    private DamageInfo TakeDamageLol(DamageInfo damageInfo) {
        float defensePerc = 100f / (100f + defense);

        DamageInfo modifiedDamageInfo = new DamageInfo(
            damageInfo.Damage[DamageType.Physical] * defensePerc,
            damageInfo.Direction,
            damageInfo.Source,
            damageInfo.SourceFaction,
            damageInfo.KnockBackStrenght
        );

        // Copia gli effetti applicati
        modifiedDamageInfo.AppliedEffects = damageInfo.AppliedEffects;

        return modifiedDamageInfo;
    }

    private DamageInfo TakeDamageOther(DamageInfo damageInfo) {
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
