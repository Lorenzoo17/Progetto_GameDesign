using UnityEngine;

/// <summary>
/// Interfaccia per gli effetti on-hit che si attivano quando un'arma colpisce un nemico.
/// 
/// Ogni implementazione di questo deve:
/// 1. Modificare il DamageInfo aggiungendo danno e flag effetti
/// 2. Comunicare quale StatusSO applicare al bersaglio
/// 3. Fornire una descrizione per l'UI
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
    /// Descrizione dell'effetto per l'UI.
    /// Usata nei tooltip dei perks.
    /// 
    /// Esempi:
    /// - "On hit: +5 Physical damage"
    /// - "On hit: Apply Poison (2.0 damage)"
    /// - "On hit: 70% chance to burn"
    /// </summary>
    string GetDescription();
}

public abstract class OnHitEffectSO : ScriptableObject, IOnHitEffect
{
    public abstract void ApplyEffect(ref DamageInfo damage, GameObject instigator);
    public abstract StatusEffectData GetAppliedStatus();
    public abstract string GetDescription();
}