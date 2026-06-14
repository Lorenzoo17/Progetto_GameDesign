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

    public override string Description()
    {
        string modValue;
        if (statType == StatType.AttackRate || statType == StatType.DodgeCoolDown)
        {
            modValue = modifierType == ModifierType.Flat
           ? value.ToString()
           : (value * -100).ToString("F0") + "%";
        }
        else
        {
            modValue = modifierType == ModifierType.Flat
            ? value.ToString()
            : (value * 100).ToString("F0") + "%";
        }
        if (value > 0 && modifierType == ModifierType.Flat)
        {
            modValue = "+" + modValue;
        }
        return $"{statType}{modValue}";
    }
}
