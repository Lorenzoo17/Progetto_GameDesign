using UnityEngine;

[CreateAssetMenu(fileName = "PuddleMutagen", menuName = "Mutagens/Puddle")]
public class PoisonPuddleMutagenSO : MutagenSO
{
    [Header("Puddle")]
    public GameObject puddlePrefab;
    public float tickDamage = 5f;
    public float tickInterval = 1f;
    public float radius = 2.5f;

    public override bool Activate(Player player, MutagenInstance instance)
    {
        if (puddlePrefab == null)
            return false;

        player.playerAttack.SpawnPuddle(
            puddlePrefab,
            duration,
            tickDamage,
            tickInterval,
            radius
        );

        return true;
    }

    public override void Tick(Player player, MutagenInstance instance, float deltaTime) { }

    public override void Deactivate(Player player, MutagenInstance instance) { }
}