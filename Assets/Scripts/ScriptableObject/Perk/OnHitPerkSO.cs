using UnityEngine;

public enum OnHitEffectType
{
    DamageBoost,
    PoisonApplication
}

[CreateAssetMenu(fileName = "new on hit perk", menuName = "ScriptableObject/OnHitPerk")]
public class OnHitPerkSO : PerkBase, IOnDealDamage
{
    [SerializeField] public OnHitEffectType effectType;
    [SerializeField] public DamageType damageBoostType = DamageType.Physical;
    [SerializeField] public float value = 1f;

    public override void OnApply(Player player)
    {
        Debug.Log($"OnHit perk applied: {effectType}");
    }

    public override void OnRemove(Player player)
    {
        Debug.Log($"OnHit perk removed: {effectType}");
    }

    public DamageInfo OnDealDamage(ref DamageInfo damage)
    {
        if (effectType == OnHitEffectType.DamageBoost)
        {
            damage.Damage[damageBoostType] += value;
            damage.AddEffect("DamageBoost");
        }
        else if (effectType == OnHitEffectType.PoisonApplication)
        {
            damage.Damage[DamageType.Poison] += value;
            damage.AddEffect("PoisonApplied");
        }

        return damage;
    }

    public override string Description()
    {
        switch (effectType)
        {
            case OnHitEffectType.DamageBoost:
                return $"On hit: +{value} {damageBoostType} damage";
            case OnHitEffectType.PoisonApplication:
                return $"On hit: Apply {value} Poison damage";
            default:
                return "Unknown on hit effect";
        }
    }
}
