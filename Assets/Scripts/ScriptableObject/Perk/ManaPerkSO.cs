using UnityEngine;

[CreateAssetMenu(fileName = "new mana perk", menuName = "ScriptableObject/ManaPerk")]
public class ManaPerkSO : PerkBase
{
    [SerializeField] private int manaValue; // positivo o negativo

    public override void OnApply(Player player)
    {
        if (player != null && player.playerMana != null)
        {
            if (manaValue > 0)
            {
                player.playerMana.IncreaseMaxMana(manaValue);
            }
            else if (manaValue < 0)
            {
                player.playerMana.DecreaseMaxMana(Mathf.Abs(manaValue));
            }
        }
    }

    public override void OnRemove(Player player)
    {
        if (player != null && player.playerMana != null)
        {
            if (manaValue > 0)
            {
                player.playerMana.DecreaseMaxMana(manaValue);
            }
            else if (manaValue < 0)
            {
                player.playerMana.IncreaseMaxMana(Mathf.Abs(manaValue));
            }
        }
    }
}
