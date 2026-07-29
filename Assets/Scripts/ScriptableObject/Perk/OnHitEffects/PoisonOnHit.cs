using UnityEngine;

/// <summary>
/// Effetto on-hit che applica lo status poison al nemico colpito.
/// Delega tutta la gestione del poison a EnemyStatus e PoisonStatusEffect.
/// </summary>
[CreateAssetMenu(fileName = "Poison On Hit", menuName = "ScriptableObject/OnHitEffect/PoisonApplication")]
public class PoisonOnHitEffect : OnHitEffectSO
{
    [SerializeField] private PoisonStatusEffect poisonStatus;

    public override void ApplyEffect(ref DamageInfo damage, GameObject instigator)
    {
        // Applica il poison tramite il flag che verrà letto in HealthSystem
        // Il danno poison viene aggiunto se il status è applicato
        if (poisonStatus != null)
        {
            damage.addEffect("PoisonApplied");
            damage.Damage[DamageType.Poison] += poisonStatus.baseValue;
        }
    }

    public override string GetDescription()
    {
        if (poisonStatus == null) return "Apply Poison (not configured)";
        return $"On hit: Apply {poisonStatus.effectName} ({poisonStatus.baseValue} damage)";
    }

    /// <summary>
    /// Getter pubblico per il status poison (usato in HealthSystem per applicare lo status)
    /// </summary>
    public override StatusEffectData GetAppliedStatus()
    {
        return poisonStatus;
    }
}