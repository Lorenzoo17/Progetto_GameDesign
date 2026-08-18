using UnityEngine;

/// <summary>
/// Interfaccia per gli effetti on-hit che si attivano quando un'arma colpisce un nemico.
/// 
/// Ogni implementazione di questo deve:
/// 1. Modificare il DamageInfo aggiungendo danno e flag effetti
/// 2. Comunicare quale StatusSO applicare al bersaglio
/// 3. Fornire il valore dello status (es: poison value, burn intensity, etc.)
/// 4. Fornire una descrizione per l'UI
/// </summary>
public interface IOnHitEffect
{
    /// <summary>
    /// Applica l'effetto al danno quando un'arma colpisce.
    /// Modifica il DamageInfo aggiungendo danno o flag.
    /// 
    /// Esempi:
    /// - damage.Damage[DamageType.Poison] += poisonStatus.baseValue;
    /// - damage.AddEffect("PoisonApplied");
    /// - damage.Damage[DamageType.Physical] += damageBoostValue;
    /// </summary>
    void ApplyEffect(ref DamageInfo damage, GameObject instigator);
    
    /// <summary>
    /// Restituisce lo StatusSO da applicare al bersaglio, o null se non c'è uno status.
    /// 
    /// HealthSystem chiama questo per sapere quale status applicare al nemico colpito.
    /// 
    /// Esempi di ritorno:
    /// - PoisonOnHitEffect → poisonStatus (StatusSO del veleno)
    /// - BurnOnHitEffect → burnStatus (StatusSO del fuoco)
    /// - DamageBoostEffect → null (niente status, solo danno aumentato)
    /// </summary>
    StatusEffectData GetAppliedStatus();
    
    /// <summary>
    /// Restituisce il valore dello status da applicare (es: poison value, burn intensity).
    /// 
    /// Questo valore verrà accumulato quando ci sono più perk dello stesso tipo
    /// e passato al metodo OnStack() dello status.
    /// 
    /// Esempi:
    /// - PoisonOnHitEffect con poison value 2 → ritorna 2f
    /// - NegativePoisonEffect (debuff) con valor -2 → ritorna -2f
    /// - DamageBoostEffect (nessuno status) → ritorna 0f
    /// </summary>
    float GetEffectValue();
    
    /// <summary>
    /// Descrizione dell'effetto per l'UI.
    /// Usata nei tooltip dei perks.
    /// 
    /// Esempi:
    /// - "On hit: +5 Physical damage"
    /// - "On hit: Apply Poison (+2.0 damage)"
    /// - "On hit: 70% chance to burn"
    /// </summary>
    string GetDescription();
}

public abstract class OnHitEffectSO : ScriptableObject, IOnHitEffect
{
    public abstract void ApplyEffect(ref DamageInfo damage, GameObject instigator);
    public abstract StatusEffectData GetAppliedStatus();
    public abstract float GetEffectValue();
    public abstract string GetDescription();
}