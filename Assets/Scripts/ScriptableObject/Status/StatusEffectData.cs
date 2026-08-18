using UnityEngine;

public enum StatusEffectType
{
    Dot,
    Buff,
    Debuff,
    CrowdControl,
    HealOverTime,
    Poison,
    Burn,
    Freeze
}

public abstract class StatusEffectData : ScriptableObject
{
    public string effectName;
    public float baseDuration;
    public StatusEffectType effectType;

    public abstract void OnApply(GameObject target);
    public abstract void OnRemove(GameObject target);

    public abstract void OnTick(GameObject target, ActiveStatusEffect activeEffect, float multiplier);
    
    /// <summary>
    /// Chiamato quando lo status viene stackato (riapplicato).
    /// 
    /// statusValue: il valore da aggiungere/modificare (es: poison value, buff amount, etc.)
    /// Per la maggior parte degli effetti, questo sarà 0f se non pertinente.
    /// </summary>
    public abstract void OnStack(GameObject target, ActiveStatusEffect activeEffect, float multiplier, int statusValue = 0);

    public virtual float GetModifiedDuration(float multiplier)
    {
        return baseDuration * multiplier;
    }

    /// <summary>
    /// Determina se l'effetto dovrebbe essere rimosso.
    /// Override in sottoclassi per custom logic (es: Poison rimuove quando poisonValue <= 0).
    /// </summary>
    public virtual bool ShouldRemove(ActiveStatusEffect activeEffect)
    {
        return activeEffect.remainingDuration <= 0f;
    }
}