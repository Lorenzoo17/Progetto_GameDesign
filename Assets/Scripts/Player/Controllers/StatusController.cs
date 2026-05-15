using System.Collections.Generic;
using UnityEngine;

public class StatusController : MonoBehaviour
{
    public List<StatusInstance> activeStatuses = new();

    private Player player;

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    private void Update()
    {
        UpdateStatuses(Time.deltaTime);
    }

    public void AddStatus(StatusSO statusSO)
    {
        if (statusSO == null) return;

        // chance check
        if (Random.value > statusSO.chance)
            return;

        // check existing
        foreach (var status in activeStatuses)
        {
            if (status.source == statusSO)
            {
                if (statusSO.stackable)
                {
                    status.AddStack();
                }
                else
                {
                    status.Refresh();
                }

                return;
            }
        }

        // create new
        StatusInstance newStatus = statusSO.CreateInstance();

        activeStatuses.Add(newStatus);

        Debug.Log($"Added status: {statusSO.statusName}");
    }

    private void UpdateStatuses(float deltaTime)
    {
        for (int i = activeStatuses.Count - 1; i >= 0; i--)
        {
            StatusInstance status = activeStatuses[i];

            status.UpdateTime(deltaTime);

            HandleStatusEffect(status, deltaTime);

            if (status.IsExpired)
            {
                Debug.Log($"Removed status: {status.source.statusName}");

                activeStatuses.RemoveAt(i);
            }
        }
    }

    private void HandleStatusEffect(StatusInstance status, float deltaTime)
    {
        if (status.source == null) return;

        switch (status.source.effectType)
        {
            case StatusEffectType.DamageOverTime:
            case StatusEffectType.Burn:
            case StatusEffectType.Poison:

                float damage = status.UpdateTick(deltaTime);

                if (damage > 0f)
                {
                    DamageInfo damageInfo = new DamageInfo(damage, Vector2.zero, player.gameObject, EntityType.Player);
                    player.playerHealth.TakeDamage(damageInfo);

                    Debug.Log($"DOT damage: {damage}");
                }

                break;

            case StatusEffectType.HealOverTime:

                float heal = status.UpdateTick(deltaTime);

                if (heal > 0f)
                {
                    player.playerHealth.Heal((int)heal);
                }

                break;

            case StatusEffectType.Slow:
                // apply slow logic
                break;

            case StatusEffectType.Stun:
                // apply stun logic
                break;
        }
    }

    public List<StatusInstance> GetStatuses()
    {
        return activeStatuses;
    }
}
