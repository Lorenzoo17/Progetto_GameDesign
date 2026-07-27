using UnityEngine;

[CreateAssetMenu(menuName = "Status Effects/Stun")]
public class StunEffectData : StatusEffectData
{
    public override void OnApply(GameObject target)
    {
        if (target.TryGetComponent<EnemyMovementNav>(out EnemyMovementNav nav))
        {
            nav.ForceStop();
        }
    }

    public override void OnTick(GameObject target, ActiveStatusEffect activeEffect, float multiplier)
    {
        // Il tick dello stun non fa nulla
    }

    public override void OnRemove(GameObject target)
    {
        if (target.TryGetComponent<EnemyMovementNav>(out var nav))
        {
            nav.ResumeMovement();
        }
    }

    public override void OnStack(GameObject target, ActiveStatusEffect activeEffect, float multiplier)
    {
        float modifiedDuration = GetModifiedDuration(multiplier);
        activeEffect.remainingDuration = Mathf.Max(activeEffect.remainingDuration, modifiedDuration);
        activeEffect.currentStacks++;
    }
}