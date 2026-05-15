using System.Collections.Generic;
using UnityEngine;
public class PerkController : MonoBehaviour
{

    public List<PerkBase> activePerks = new();

    // Filtered lists (performance + clarity)
    public List<IModifyIncomingDamage> incomingDamageModifiers = new();
    public List<IOnDealDamage> onDealDamageEffects = new();

    public Player player;

    public void Awake()
    {
        player = GetComponent<Player>();
    }

    // ---------------- ADD / REMOVE ----------------

    public void AddPerk(PerkBase perk)
    {
        activePerks.Add(perk);
        perk.OnApply(player);

        // Register interfaces
        if (perk is IModifyIncomingDamage dmgMod)
            incomingDamageModifiers.Add(dmgMod);

        if (perk is IOnDealDamage dealDmg)
            onDealDamageEffects.Add(dealDmg);
    }

    public void RemovePerk(PerkBase perk)
    {
        if (!activePerks.Contains(perk)) return;

        activePerks.Remove(perk);
        perk.OnRemove(player);

        if (perk is IModifyIncomingDamage dmgMod)
            incomingDamageModifiers.Remove(dmgMod);

        if (perk is IOnDealDamage dealDmg)
            onDealDamageEffects.Remove(dealDmg);
    }

    // ---------------- HOOKS ----------------

    public int ModifyIncomingDamage(int damage)
    {
        foreach (var mod in incomingDamageModifiers)
        {
            damage = mod.ModifyIncomingDamage(damage);
        }
        return damage;
    }

    public DamageInfo OnDealDamage(ref DamageInfo damage)
    {
        foreach (var effect in onDealDamageEffects)
        {
            damage = effect.OnDealDamage(ref damage);
        }
        return damage;
    }

    public List<PerkBase> GetActivePerks()
    {
        return activePerks;
    }
}

/// INTERFACES FOR PERKS
public interface IModifyIncomingDamage
{
    int ModifyIncomingDamage(int damage);
}
public interface IOnDealDamage
{
    public DamageInfo OnDealDamage(ref DamageInfo damage);
}
