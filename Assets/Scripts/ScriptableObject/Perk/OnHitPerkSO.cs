using UnityEngine;

/// <summary>
/// Perk che applica effetti on-hit tramite il sistema IOnHitEffect.
/// 
/// Implementa sia IOnDealDamage (per modificare il danno) che IOnHitEffect 
/// (per comunicare quale status applicare al bersaglio e con quale valore).
/// 
/// PerkController si occupa automaticamente di registrare questo perk
/// e mantenerlo nella lista di effetti on-deal-damage.
/// 
/// Nel nuovo sistema, quando il giocatore attacca:
/// 1. OnDealDamage() viene chiamato per modificare il DamageInfo
/// 2. HealthSystem somma i valori di tutti i perk on-hit (GetEffectValue())
/// 3. HealthSystem applica gli status accumulati al nemico con i valori totali
/// </summary>
[CreateAssetMenu(fileName = "new on hit perk", menuName = "ScriptableObject/OnHitPerk")]
public class OnHitPerkSO : PerkBase, IOnDealDamage, IOnHitEffect
{
    [SerializeField] private OnHitEffectSO effect;
    
    public override void OnApply(Player player)
    {
        Debug.Log($"OnHit perk applied: {(effect != null ? effect.GetType().Name : "No effect configured")}");
    }

    public override void OnRemove(Player player)
    {
        Debug.Log($"OnHit perk removed: {(effect != null ? effect.GetType().Name : "No effect configured")}");
    }

    // ========== IOnDealDamage ==========
    /// <summary>
    /// Modifica il DamageInfo quando il giocatore attacca.
    /// Nel nuovo sistema, questo viene usato principalmente per flag effetti,
    /// mentre i valori di status vengono gestiti da HealthSystem.ApplyStatusEffectsFromSource().
    /// </summary>
    public DamageInfo OnDealDamage(ref DamageInfo damage)
    {
        if (effect != null)
        {
            effect.ApplyEffect(ref damage, damage.Source);
        }
        else
        {
            Debug.LogWarning("[OnHitPerkSO] Effect non configurato!");
        }

        return damage;
    }

    // ========== IOnHitEffect ==========
    
    public void ApplyEffect(ref DamageInfo damage, GameObject instigator)
    {
        if (effect != null)
        {
            effect.ApplyEffect(ref damage, instigator);
        }
    }

    public StatusEffectData GetAppliedStatus()
    {
        // Questo è il metodo che HealthSystem chiama per sapere quale status applicare
        if (effect != null)
        {
            return effect.GetAppliedStatus();
        }
        return null;
    }

    /// <summary>
    /// Ritorna il valore dello status da applicare.
    /// Nel nuovo sistema, HealthSystem somma i valori di tutti i perk on-hit
    /// prima di applicare lo status al nemico.
    /// </summary>
    public float GetEffectValue()
    {
        if (effect != null)
        {
            return effect.GetEffectValue();
        }
        return 0f;
    }

    // ========== UI ==========

    // perk related description for UI
    public override string Description()
    {
        return effect != null ? effect.GetDescription() : "Unknown on hit effect";
    }
    
    // status related description for UI
    public string GetDescription()
    {
        return effect != null ? effect.GetDescription() : "Unknown on hit effect";
    }
}