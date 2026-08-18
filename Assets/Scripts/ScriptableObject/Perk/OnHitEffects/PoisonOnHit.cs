using UnityEngine;

/// <summary>
/// Effetto on-hit che applica lo status poison al nemico colpito.
/// 
/// Il valore del poison è configurabile da Inspector e rappresenta quanto "potenziale di danno"
/// questo perk applica al nemico.
/// 
/// Se hai multiple perk poison, i loro valori si sommeranno.
/// Se hai perk poison positivi e negativi, se ne faranno la somma (es: +2 -2 = 0, nessun poison applicato).
/// </summary>
[CreateAssetMenu(fileName = "Poison On Hit", menuName = "ScriptableObject/OnHitEffect/PoisonApplication")]
public class PoisonOnHitEffect : OnHitEffectSO
{
    [SerializeField] private PoisonStatusEffect poisonStatus;
    
    [Header("Poison Value")]
    [SerializeField] private float poisonValue = 2f;
    [TextArea] [SerializeField] private string valueDescription = "Il valore di danno poison applicato al nemico. Può essere negativo (debuff).";

    public override void ApplyEffect(ref DamageInfo damage, GameObject instigator)
    {
        // Nel nuovo sistema, il danno poison non è aggiunto subito qui.
        // Viene gestito completamente dal sistema di status tramite EnemyStatus.
        // Questo metodo esiste solo per mantenere la compatibilità con IOnDealDamage.
        
        if (poisonStatus != null)
        {
            damage.addEffect("PoisonApplied");
            // Nota: il danno effettivo verrà calcolato quando lo status viene applicato
        }
    }

    public override StatusEffectData GetAppliedStatus()
    {
        return poisonStatus;
    }

    /// <summary>
    /// Ritorna il valore di poison da applicare.
    /// Questo valore verrà sommato con gli altri perk poison quando il giocatore attacca.
    /// </summary>
    public override float GetEffectValue()
    {
        return poisonValue;
    }

    public override string GetDescription()
    {
        if (poisonStatus == null) 
            return "Apply Poison (not configured)";
        
        string valueStr = poisonValue >= 0 ? $"+{poisonValue}" : poisonValue.ToString();
        return $"On hit: Apply {poisonStatus.effectName} ({valueStr} damage/tick)";
    }
}