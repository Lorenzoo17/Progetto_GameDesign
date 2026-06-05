using UnityEngine;
[CreateAssetMenu(fileName = "HealthPerk", menuName = "ScriptableObject/Perk/HealthPerk")]
public class HealthPerk : PerkBase
{
    public int healthIncrease;

    public HealthPerk(string name, int healthIncrease)
    {
        this.name = name;
        this.healthIncrease = healthIncrease;
    }

    public override void OnApply(Player player)
    {
        if (player.playerHealth.currentHealthUnits + healthIncrease > 0 && healthIncrease < 0) player.playerHealth.DecreaseHealth(Mathf.Abs(healthIncrease)); // applico se non uccide il player
        else if (healthIncrease > 0)
            player.playerHealth.IncreaseHealth(healthIncrease);
    }

    public override void OnRemove(Player player)
    {
        if (healthIncrease > 0) player.playerHealth.DecreaseHealth(healthIncrease);
        else
            player.playerHealth.IncreaseHealth(Mathf.Abs(healthIncrease)); // aggiungoi salute
    }
    public override string Descriptor()
    {
        if (healthIncrease > 0)
        {
            return $"Increase max health by {healthIncrease}.";
        }
        else if (healthIncrease < 0)
        {
            return $"Decrease max health by {Mathf.Abs(healthIncrease)}.";
        }
        else
        {
            return "No effect on health.";
        }
    }
}
