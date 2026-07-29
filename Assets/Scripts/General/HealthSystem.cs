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

public class HealthSystem : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth;
    public float CurrentHealth { get; private set; }
    public bool isBoss = false;
 
    public LevelLoader levelLoader;
 
    [SerializeField] private float defense;
    public event EventHandler<DamageEventArgs> OnDamageTaken;
    public event EventHandler<BossDeathEventArgs> OnBossDeath;
 
    private DamageEffectVisuals damageEffectVisuals;
    private EnemyStatus enemyStatus;
 
    private void Awake()
    {
        CurrentHealth = maxHealth;
        defense = 0;
        damageEffectVisuals = GetComponent<DamageEffectVisuals>();
        enemyStatus = GetComponent<EnemyStatus>();
    }
 
    public float GetHealthPercentage()
    {
        return (CurrentHealth / maxHealth) * 100f;
    }
 
    public void TakeDamage(DamageInfo damageInfo)
    {
        // Screen shake quando l'entità prende danno
        if (EffectManager.Instance != null)
        {
            CameraShakerHandler.Shake(EffectManager.Instance.GetShakeDataByType(ShakeDataType.MeleeAttack));
        }
 
        // Applica danno fisico
        ApplyPhysicalDamage(damageInfo);
 
        // Applica gli status dai perk on-hit dell'attaccante
        ApplyStatusEffectsFromSource(damageInfo);
    }
 
    private void ApplyPhysicalDamage(DamageInfo damageInfo)
    {
        DamageInfo modifiedDamageInfo = TakeDamageLol(damageInfo);
        CurrentHealth -= modifiedDamageInfo.Damage[DamageType.Physical];
        OnDamageTaken?.Invoke(this, new DamageEventArgs(
            modifiedDamageInfo.Damage[DamageType.Physical],
            modifiedDamageInfo.Direction,
            modifiedDamageInfo.KnockBackStrenght,
            DamageType.Physical));
        HandleDamageVisuals(modifiedDamageInfo);
 
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound2D(SoundID.EnemyHit, .25f);
        }
        HandleDeathIfNeeded();
    }
 
    /// <summary>
    /// Applica gli status dai perk on-hit dell'attaccante.
    /// 
    /// Chiede al PerkController del giocatore:
    /// "Dammi tutti i perk IOnDealDamage attivi, e per ogni uno chiedimi
    /// quale StatusSO dovrei applicare al nemico colpito."
    /// </summary>
    private void ApplyStatusEffectsFromSource(DamageInfo damageInfo)
    {
        if (damageInfo.Source == null || enemyStatus == null)
            return;
 
        // Prova a trovare il PerkController
        PerkController perkController = damageInfo.Source.GetComponent<PerkController>();
        if (perkController == null)
        {
            perkController = damageInfo.Source.transform.root.GetComponent<PerkController>();
        }
 
        if (perkController == null)
        {
            // Non è un giocatore/entità con perks
            return;
        }
 
        // Scorri tutti i perk on-deal-damage attivi
        foreach (IOnDealDamage dealDamage in perkController.onDealDamageEffects)
        {
            // Verifica se il perk implementa anche IOnHitEffect
            if (dealDamage is IOnHitEffect onHitEffect)
            {
                StatusEffectData status = onHitEffect.GetAppliedStatus();
                
                if (status != null)
                {
                    Debug.Log($"[HealthSystem] Applying {status.effectName} to {gameObject.name}");
                    enemyStatus.ApplyEffect(status);
                    
                    // Trigga effetto visuale
                    HandleStatusVisuals(status);
                }
            }
        }
    }
 
    /// <summary>
    /// Trigga l'effetto visuale corrispondente allo status.
    /// </summary>
    private void HandleStatusVisuals(StatusEffectData status)
    {
        if (damageEffectVisuals == null)
            return;
 
        switch (status.effectType)
        {
            case StatusEffectType.Poison:
                damageEffectVisuals.PlayPoisonEffect();
                break;
            case StatusEffectType.Burn:
                // Se hai un effetto burn, mettilo qui
                // damageEffectVisuals.PlayBurnEffect();
                break;
            case StatusEffectType.Freeze:
                // damageEffectVisuals.PlayFreezeEffect();
                break;
            // Aggiungi altri status secondo necessità
        }
    }
 
    private void HandleDeathIfNeeded()
    {
        if (CurrentHealth <= 0)
        {
            Debug.Log($"[HealthSystem] Enemy died!");
 
            if (isBoss)
            {
                GameObject boss = GameObject.FindGameObjectWithTag("Racit");
                Debug.Log($"Questo è il boss: {boss}");
 
                if (boss != null)
                {
                    Debug.Log($"Pronto per il check al trofeo");
                    PipeManager manager = FindAnyObjectByType<PipeManager>();
 
                    Debug.Log($"Tubi chiusi? {manager.isAllLocked()}");
                    if (manager.isAllLocked())
                    {
                        TrophieManager.isRacitTrophieUnlocked = true;
                        Debug.Log($"Trofeo Ottenuto");
                    }
                }
                FindFirstObjectByType<PerkController>()?.ClearAllNegativePerks();
            }
 
            Destroy(gameObject);
        }
    }
 
    private void HandleDamageVisuals(DamageInfo damageInfo)
    {
        if (damageEffectVisuals == null)
            return;
 
        // Particella rossa se il danno è stato boosted
        if (damageInfo.AppliedEffects.Contains("DamageBoost"))
        {
            damageEffectVisuals.PlayDamageBoostEffect();
        }
    }
 
    private DamageInfo TakeDamageLol(DamageInfo damageInfo)
    {
        float defensePerc = 100f / (100f + defense);

        DamageInfo modifiedDamageInfo = new DamageInfo(
            damageInfo.Damage[DamageType.Physical] * defensePerc,
            damageInfo.Direction,
            damageInfo.Source,
            damageInfo.SourceFaction,
            damageInfo.KnockBackStrenght
        )
        {
            // Copia gli effetti applicati
            AppliedEffects = damageInfo.AppliedEffects
        };

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
        )
        {
            AppliedEffects = damageInfo.AppliedEffects
        };

        return modifiedDamageInfo;
    }
    public void TakePoisonDamage(DamageInfo damageInfo)
    {
        if (enemyStatus == null)
            return;

        // Applica lo status di veleno
        StatusEffectData poisonStatus = damageInfo.AppliedStatus;
        if (poisonStatus != null && poisonStatus.effectType == StatusEffectType.Poison)
        {
            Debug.Log($"[HealthSystem] Applying/ {poisonStatus.effectName} to {gameObject.name}");
            enemyStatus.ApplyEffect(poisonStatus);

            // Trigga effetto visuale
            HandleStatusVisuals(poisonStatus);
        }

        // Applica il danno di veleno alla salute
        CurrentHealth -= damageInfo.Damage[DamageType.Poison];
        OnDamageTaken?.Invoke(this, new DamageEventArgs(
            damageInfo.Damage[DamageType.Poison],
            damageInfo.Direction,
            damageInfo.KnockBackStrenght,
            DamageType.Poison));

        HandleDeathIfNeeded();
    }
}
 
