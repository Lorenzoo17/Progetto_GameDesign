using UnityEngine;

/// <summary>
/// Effetto on-hit che aumenta il danno di un tipo specifico.
/// </summary>
[CreateAssetMenu(fileName = "Damage Boost On Hit", menuName = "ScriptableObject/OnHitEffect/DamageBoost")]
public class DamageBoostOnHitEffect : OnHitEffectSO
{
    [SerializeField] private DamageType damageType = DamageType.Physical;
    [SerializeField] private float damageIncrease = 5f;

    public override void ApplyEffect(ref DamageInfo damage, GameObject instigator)
    {
        damage.Damage[damageType] += damageIncrease;
        damage.addEffect("DamageBoost");
    }

    public override string GetDescription()
    {
        return $"On hit: +{damageIncrease} {damageType} damage";
    }
    public override StatusEffectData GetAppliedStatus()
    {
        // Questo effetto non applica alcuno status, quindi restituisce null
        return null;
    }

    public override float GetEffectValue()
    {
        return damageIncrease;
    }
}