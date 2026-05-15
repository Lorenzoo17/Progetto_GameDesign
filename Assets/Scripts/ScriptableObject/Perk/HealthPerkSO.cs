using UnityEngine;
[CreateAssetMenu(fileName = "HealthPerk", menuName = "ScriptableObject/Perk/HealthPerk")]
public class HealthPerk : PerkBase
{
    private int healthIncrease;

    public HealthPerk(string name, int healthIncrease)
    {
        this.name = name;
        this.healthIncrease = healthIncrease;
    }

    public override void OnApply(Player player)
    {
        player.playerHealth.IncreaseHealth(healthIncrease);
    }

    public override void OnRemove(Player player)
    {
        player.playerHealth.DecreaseHealth(healthIncrease);
    }
}
