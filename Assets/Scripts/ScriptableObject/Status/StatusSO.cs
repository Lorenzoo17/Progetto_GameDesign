using UnityEngine;

public enum StatusEffectType
{
    None,
    InstantDamage,
    DamageOverTime,
    HealOverTime,
    Blind,
    Slow,
    Stun,
    Silence,
    Root,
    Confuse,
    Freeze,
    Burn,
    Poison,
    Regeneration,
    Shield
}

public enum StatusEffectCategory
{
    None,
    Damage,
    Debuff,
    Buff,
    Control
}

public enum DamageType
{
    None,
    Physical,
    Magical,
    Fire,
    Ice,
    Poison,
    True
}

[CreateAssetMenu(fileName = "New Status", menuName = "ScriptableObject/Status/StatusSO")]
public class StatusSO : ScriptableObject
{

    [Header("Base info")]
    public string statusName = "New Status";
    [TextArea(2, 5)]
    public string description;
    public StatusEffectCategory category = StatusEffectCategory.Debuff;
    public StatusEffectType effectType = StatusEffectType.None;
    public bool isVisible = true;

    [Header("Damage / healing")]
    public DamageType damageType = DamageType.None;
    public float baseValue = 1f;
    public float duration = 3f;
    public float tickInterval = 1f;

    [Header("Application")]
    [Range(0f, 1f)] public float chance = 1f;
    public bool stackable = false;
    public int maxStacks = 1;
    public bool isPermanent = false;

    public bool HasDamage => effectType == StatusEffectType.InstantDamage || effectType == StatusEffectType.DamageOverTime || effectType == StatusEffectType.Burn || effectType == StatusEffectType.Poison || effectType == StatusEffectType.Freeze;
    public bool HasAbstractEffect => effectType == StatusEffectType.Blind || effectType == StatusEffectType.Slow || effectType == StatusEffectType.Stun || effectType == StatusEffectType.Silence || effectType == StatusEffectType.Root || effectType == StatusEffectType.Confuse || effectType == StatusEffectType.Shield || effectType == StatusEffectType.Regeneration;

    public StatusInstance CreateInstance()
    {
        return new StatusInstance(this);
    }

    public string GetTooltip()
    {
        string tooltip = statusName;
        if (!string.IsNullOrWhiteSpace(description))
        {
            tooltip += " - " + description;
        }
        tooltip += $"\nTipo: {effectType}";
        if (HasDamage)
        {
            tooltip += $"\nDanno: {baseValue} ({damageType})";
        }
        if (duration > 0f && !isPermanent)
        {
            tooltip += $"\nDurata: {duration}s";
        }
        if (tickInterval > 0f && effectType == StatusEffectType.DamageOverTime)
        {
            tooltip += $"\nTick ogni {tickInterval}s";
        }
        if (stackable)
        {
            tooltip += $"\nStackable fino a {maxStacks}";
        }
        return tooltip;
    }
}
