using UnityEngine;

[CreateAssetMenu(fileName = "NewAOEData", menuName = "Combat/AOE Data")]
public class AOEMutagenSO : MutagenSO
{
    [Header("Visual & Sound")]
    public ShakeDataType shakeType = ShakeDataType.MeleeAttack;
    public SoundID soundEffect = SoundID.PlayerAttack;

    [Header("Stats")]
    public float radius = 3f;            // Raggio dell'esplosione
    public float damageMultiplier = 2f;  // Moltiplicatore del danno base del player
    public float cooldown = 5f;          // Tempo di attesa tra un attacco e l'altro


    public override void Activate(Player player, MutagenInstance instance)
    {
        // player.playerAttack.triggerAOE(player);
    }

    public override void Tick(
        Player player,
        MutagenInstance instance,
        float deltaTime)
    {

    }

    public override void Deactivate(
        Player player,
        MutagenInstance instance)
    {

    }
}
