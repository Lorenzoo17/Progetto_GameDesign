using UnityEngine;

/// <summary>
/// Implementazione dello StatusEffectData per il veleno.
/// Gestisce i danni cumulativi, l'accumulo di stack, e i tick di danno.
/// </summary>
[CreateAssetMenu(fileName = "Poison Status", menuName = "ScriptableObject/Status/PoisonStatusEffect")]
public class PoisonStatusEffect : StatusEffectData
{
    [Header("Poison Specific")]
    [SerializeField] private DamageType damageType = DamageType.Poison;
    [SerializeField] private float tickInterval = 1f;
    [SerializeField] private bool scaleDamageWithStacks = true;

    [SerializeField] public float baseValue = 1f; // Danno base per tick

    private float tickTimer = 0f;

    public override void OnApply(GameObject target)
    {
        Debug.Log($"[PoisonStatusEffect] Applied to {target.name}");
        
        // Effetto visuale del poison (se esiste il componente)
        if (target.TryGetComponent<DamageEffectVisuals>(out DamageEffectVisuals visuals))
        {
            visuals.PlayPoisonEffect();
        }
        
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound3D(SoundID.PoisonApplied, target.transform.position);
        }
    }

    public override void OnRemove(GameObject target)
    {
        Debug.Log($"[PoisonStatusEffect] Removed from {target.name}");
        
        // Eventuali effetti di rimozione (fade out visuale, suono, etc.)
        if (target.TryGetComponent<DamageEffectVisuals>(out DamageEffectVisuals visuals))
        {
            // Potresti aggiungere qui un metodo PlayPoisonFade() se desiderato
        }
    }

    public override void OnTick(GameObject target, ActiveStatusEffect activeEffect, float multiplier)
    {
        if (target == null) return;

        tickTimer += Time.deltaTime;
        
        if (tickTimer >= tickInterval)
        {
            // Calcola il danno con multiplier (resistenza/debolezza del bersaglio)
            float damagePerTick = GetDamagePerTick(activeEffect, multiplier);
            tickTimer = 0f;

            // Applica il danno direttamente alla salute
            if (target.TryGetComponent<IDamageable>(out IDamageable damageable))
            {
                DamageInfo poisonDamage = new DamageInfo(
                    damagePerTick,
                    Vector2.zero, // Il veleno non ha direzione
                    target, // Il source è il target stesso (non è "arrivato da fuori")
                    EntityType.Enemy
                );
                poisonDamage.Damage[damageType] = damagePerTick;
                poisonDamage.addEffect("PoisonTick");

                damageable.TakeDamage(poisonDamage);
            }

            Debug.Log($"[PoisonStatusEffect] Tick damage applied: {damagePerTick} to {target.name}, Stacks: {activeEffect.currentStacks}");
        }
    }

    public override void OnStack(GameObject target, ActiveStatusEffect activeEffect, float multiplier)
    {
        if (activeEffect.currentStacks < 5) // Max 5 stack di veleno
        {
            activeEffect.currentStacks++;
            activeEffect.remainingDuration = baseDuration * multiplier;
            
            Debug.Log($"[PoisonStatusEffect] Stacked! New stacks: {activeEffect.currentStacks} on {target.name}");
            
            // Effetto visuale quando stacka
            if (target.TryGetComponent<DamageEffectVisuals>(out DamageEffectVisuals visuals))
            {
                visuals.PlayPoisonEffect();
            }
        }
    }

    private float GetDamagePerTick(ActiveStatusEffect activeEffect, float multiplier)
    {
        float baseDamage = baseValue;
        
        // Se scaleDamageWithStacks è true, il danno aumenta con gli stack
        if (scaleDamageWithStacks)
        {
            baseDamage *= activeEffect.currentStacks;
        }
        
        // Applica il moltiplicatore (resistenza/debolezza)
        return baseDamage * multiplier;
    }

    public override float GetModifiedDuration(float multiplier)
    {
        return baseDuration * multiplier;
    }
}