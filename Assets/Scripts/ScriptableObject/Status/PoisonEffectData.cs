using UnityEngine;

/// <summary>
/// Implementazione dello StatusEffectData per il veleno con sistema di accumulo valori.
/// 
/// Il poison funziona come segue:
/// - Ogni perk poison ha un valore (es: +2, -2, etc.)
/// - Quando attacchi, i valori di tutti i perk poison si sommano
/// - Questo valore cumulativo viene applicato al nemico come "poison value"
/// - Ad ogni tick, il nemico prende danno = poison value (moltiplicato per resistenza/debolezza)
/// - Dopo il danno, il poison value decresce di 1
/// - Quando riattacchi prima che finisca, il nuovo valore si somma al poison value esistente
/// 
/// Esempi:
/// - Attacco con perk +2: poison value = 2 → danno 2, poi 1, poi 0 (fine)
/// - Attacco con perk +2 e +2: poison value = 4 → danno 4, 3, 2, 1, 0
/// - Attacco quando poison value è 3, con perk +2: poison value = 5 → danno 5, 4, 3, 2, 1, 0
/// - Attacco con +2 e -2: poison value = 0 (non applica il poison)
/// </summary>
[CreateAssetMenu(fileName = "Poison Status", menuName = "ScriptableObject/Status/PoisonStatusEffect")]
public class PoisonStatusEffect : StatusEffectData
{
    [Header("Poison Specific")]
    [SerializeField] private DamageType damageType = DamageType.Poison;
    [SerializeField] private float tickInterval = 0.3f;

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
        if (target == null || activeEffect.currentStacks <= 0f)
            return;

        // Ogni effetto su ogni nemico ha il suo timer indipendente!
        activeEffect.tickTimer += Time.deltaTime;
        
        if (activeEffect.tickTimer >= tickInterval)
        {
            // Il danno è il valore corrente del poison, moltiplicato per il moltiplicatore
            float damagePerTick = activeEffect.currentStacks * multiplier;
            activeEffect.tickTimer = 0f;  // Reset il timer di QUESTO effetto

            // Applica il danno direttamente alla salute
            if (target.TryGetComponent<IDamageable>(out IDamageable damageable))
            {
                DamageInfo poisonDamage = new DamageInfo(
                    damagePerTick,
                    Vector2.zero, // Il veleno non ha direzione
                    target,       // Il source è il target stesso
                    EntityType.Enemy
                );
                poisonDamage.Damage[damageType] = damagePerTick;
                poisonDamage.addEffect("PoisonTick");
                // Effetto visuale del poison (se esiste il componente)
                if (target.TryGetComponent<DamageEffectVisuals>(out DamageEffectVisuals visuals))
                {
                    visuals.PlayPoisonEffect();
                }
                damageable.TakePoisonDamage(poisonDamage);    
            }

            // IMPORTANTE: Decrementa il valore del poison dopo il danno
            activeEffect.currentStacks -= 1;

            Debug.Log($"[PoisonStatusEffect] Tick damage applied: {damagePerTick} to {target.name}, Poison Value: {activeEffect.currentStacks}");
        }
    }

    public override void OnStack(GameObject target, ActiveStatusEffect activeEffect, float multiplier, int statusValue = 0)
    {
        // Aggiungi il nuovo valore al poison value esistente
        activeEffect.currentStacks += statusValue;
        
        // Estendi la durata quando stackiamo (così il poison ha tempo di consumarsi)
        activeEffect.remainingDuration = baseDuration * multiplier;
        
        Debug.Log($"[PoisonStatusEffect] Stacked! New poison value: {activeEffect.currentStacks} on {target.name}");
        
        // Effetto visuale quando stacka
        if (target.TryGetComponent<DamageEffectVisuals>(out DamageEffectVisuals visuals))
        {
            visuals.PlayPoisonEffect();
        }
    }

    public override float GetModifiedDuration(float multiplier)
    {
        return baseDuration * multiplier;
    }

    /// <summary>
    /// Override di ShouldRemove per il poison.
    /// Il poison viene rimosso quando:
    /// - Il suo valore scende a 0 o sotto, OPPURE
    /// - La durata massima viene superata (timeout di sicurezza)
    /// </summary>
    public override bool ShouldRemove(ActiveStatusEffect activeEffect)
    {
        return base.ShouldRemove(activeEffect) || activeEffect.currentStacks <= 0f;
    }
}