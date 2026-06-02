using UnityEngine;

[CreateAssetMenu(fileName = "new stat perk", menuName = "ScriptableObject/StatPerk")]
public class StatPerkSO : PerkBase
{
    public StatType statType;
    public ModifierType modifierType;
    public float value;

    public override void OnApply(Player player)
    {
        if (player != null && player.playerStats != null)
        {
            player.playerStats.AddPerk(this);
        }
        else
        {
            Debug.LogError("Player o PlayerStats non trovati!");
        }
    }

    public override void OnRemove(Player player)
    {
        if (player != null && player.playerStats != null)
        {
            player.playerStats.RemovePerk(this);
        }
        else
        {
            Debug.LogError("Player o PlayerStats non trovati!");
        }
    }
}
