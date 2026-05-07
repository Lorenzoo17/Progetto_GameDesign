using UnityEngine;

[CreateAssetMenu(fileName = "new on hit perk", menuName = "ScriptableObject/OnHitPerk")]
public class OnHitPerkSO : PerkBase, IOnDealDamage
{

    public ModifierType modifierType;
    public float value;
    DamageType damageBoostType;
    TypeOfOnHitPerk typeOfOnHitPerk;

    public override void OnApply(Player player)
    {
        Debug.Log("OnHit perk applied");
    }

    public override void OnRemove(Player player)
    {
        Debug.Log("OnHit perk removed");
    }

    DamageInfo IOnDealDamage.OnDealDamage(ref DamageInfo damage)
    {
        damage.Damage[damageBoostType] += value;
        return damage;
    }
}

public enum TypeOfOnHitPerk
{
    None,
    DamageType,
    ApplyStatus
}
