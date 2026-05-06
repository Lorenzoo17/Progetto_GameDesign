using UnityEngine;

[CreateAssetMenu(fileName = "HealthBuff",menuName = "ScriptableObject/Buff/HealthBuff")]
public class HealthBuff : iBuff, ScriptableObject {

    public string name { get; private set; }
    private int healthIncrease;

    public HealthBuff(string name, int healthIncrease) {
        this.name = name;
        this.healthIncrease = healthIncrease;
    }

    public void ApplyBuff(Player player) {
        player.playerHealth.IncreaseHealth(healthIncrease);
    }

    public void RemoveBuff(Player player) {
        player.playerHealth.DecreaseHealth(healthIncrease);
    }
}