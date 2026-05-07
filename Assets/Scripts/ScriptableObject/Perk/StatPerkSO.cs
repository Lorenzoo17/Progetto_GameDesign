using UnityEngine;

[CreateAssetMenu(fileName = "new stat perk", menuName = "ScriptableObject/StatPerk")]
public class StatPerkSO : PerkBase
{
    public StatType statType;
    public ModifierType modifierType;
    public float value;

    public override void OnApply(Player player)
    {
        Debug.Log("Stat perk applied");
    }

    public override void OnRemove(Player player)
    {
        Debug.Log("Stat perk removed");
    }
}

