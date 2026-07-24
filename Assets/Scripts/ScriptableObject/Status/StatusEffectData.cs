using UnityEngine;


public abstract class StatusEffectData : ScriptableObject
{
    public string effectName;
    public float baseDuration;
    public StatusEffectType effectType;

    public abstract void OnApply(GameObject target);
    public abstract void OnRemove(GameObject target);

    public abstract void OnTick(GameObject target, ActiveStatusEffect activeEffect, float multiplier);
    public abstract void OnStack(GameObject target, ActiveStatusEffect activeEffect, float multiplier);

    public virtual float GetModifiedDuration(float multiplier)
    {
        return baseDuration * multiplier;
    }
}