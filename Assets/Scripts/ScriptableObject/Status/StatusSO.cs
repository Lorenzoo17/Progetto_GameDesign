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

[System.Serializable]
public class StatusInstance
{
    public StatusSO source;
    public float remainingDuration;
    public int stacks;
    public float tickTimer;

    public StatusInstance(StatusSO source)
    {
        this.source = source;
        this.remainingDuration = source == null || source.isPermanent ? Mathf.Infinity : source.duration;
        this.stacks = source != null && source.stackable ? 1 : 1;
        this.tickTimer = 0f;
    }

    public bool IsExpired => source != null && !source.isPermanent && remainingDuration <= 0f;

    public void Refresh()
    {
        if (source == null) return;
        remainingDuration = source.isPermanent ? Mathf.Infinity : source.duration;
        tickTimer = 0f;
        stacks = Mathf.Clamp(stacks, 1, source.maxStacks);
    }

    public void AddStack()
    {
        if (source == null || !source.stackable) return;
        stacks = Mathf.Min(stacks + 1, source.maxStacks);
        Refresh();
    }

    public float GetEffectiveValue()
    {
        if (source == null) return 0f;
        return source.baseValue * stacks;
    }

    public float UpdateTick(float deltaTime)
    {
        if (source == null || !source.HasDamage || source.tickInterval <= 0f) return 0f;

        tickTimer += deltaTime;
        float damage = 0f;
        while (tickTimer >= source.tickInterval)
        {
            damage += GetEffectiveValue();
            tickTimer -= source.tickInterval;
        }
        return damage;
    }

    public void UpdateTime(float deltaTime)
    {
        if (source == null || source.isPermanent) return;
        remainingDuration -= deltaTime;
    }
}
