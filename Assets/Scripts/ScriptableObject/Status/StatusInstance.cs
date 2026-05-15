using UnityEngine;
using System;



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
