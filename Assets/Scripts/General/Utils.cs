using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public enum EntityType
{
    Player,
    Enemy,
    Neutral
}
public struct SingleDamageInfo
{
    public float Amount { get; private set; }
    public DamageType Type { get; private set; }

    public SingleDamageInfo(float amount, DamageType type)
    {
        Amount = amount;
        Type = type;
    }
}


[System.Serializable]
public class DamageInfo
{
    public Dictionary<DamageType, float> Damage { get; set; }
    public Vector2 Direction { get; set; }
    public float KnockBackStrenght { get; set; }
    public GameObject Source { get; set; }
    public EntityType SourceFaction { get; set; }

    public StatusEffectData AppliedStatus { get; set; }
    public List<string> AppliedEffects { get; internal set; }

    public DamageInfo(float damage, Vector2 direction, GameObject source, EntityType sourceFaction, float knockBackStrenght = 0f, StatusEffectData status = null)
    {
        Damage = new Dictionary<DamageType, float>
        {
            { DamageType.Physical, damage },
            { DamageType.Poison, 0f }
        };
        Direction = direction;
        KnockBackStrenght = knockBackStrenght;
        Source = source;
        AppliedStatus = status;
        SourceFaction = sourceFaction;
    }


    public void addEffect(string effectName)
    {
        if (AppliedEffects == null)
        {
            AppliedEffects = new List<string>();
        }

        if (AppliedEffects is List<string> effectsList)
        {
            effectsList.Add(effectName);
        }
    }
}

public enum DamageType
{
    Physical,
    Fire,
    Ice,
    Lightning,
    Poison
}

public static class Utils
{
    public static IEnumerator FreezeFrame(float duration)
    {
        Time.timeScale = 0f;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = 1f;
    }

    public static class CombatUtility
    {
        public static bool CanDamage(GameObject attacker, GameObject target)
        {
            if (attacker == null || target == null) return false;
            if (attacker == target) return false;

            EntityOwner attackerOwner = attacker.GetComponent<EntityOwner>();
            EntityOwner targetOwner = target.GetComponent<EntityOwner>();

            if (attackerOwner == null || targetOwner == null) return true;

            return attackerOwner.GetEntityType != targetOwner.GetEntityType;
        }
    }
}
