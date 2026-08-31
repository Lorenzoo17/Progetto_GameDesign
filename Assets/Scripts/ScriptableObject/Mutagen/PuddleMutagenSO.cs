using UnityEngine;

[CreateAssetMenu(fileName = "PuddleMutagen", menuName = "Mutagens/Puddle")]
public class PoisonPuddleMutagenSO : MutagenSO
{
    [Header("Puddle")]
    public GameObject puddlePrefab;
    public float tickDamage = 5f;
    public float tickInterval = 1f;
    public float radius = 2.5f;
    public float damageMultiplier = 1f;

    public override bool Activate(Player player, MutagenInstance instance)
    {
        if (puddlePrefab == null)
            return false;

        player.playerAttack.SpawnPuddle(
            puddlePrefab,
            duration,
            tickDamage + player.playerStats.playerCurrentStats.GetMutagenPower() * damageMultiplier,
            tickInterval,
            radius
        );

        return true;
    }

    public override void Tick(Player player, MutagenInstance instance, float deltaTime) { }

    public override void Deactivate(Player player, MutagenInstance instance) { }

    public override string Description()
    {
        return $"Create a toxic puddle at your location that lasts for {duration} seconds. Enemies that step into the puddle take {tickDamage} damage every {tickInterval} seconds. The puddle has a radius of {radius} units.";
    }
}