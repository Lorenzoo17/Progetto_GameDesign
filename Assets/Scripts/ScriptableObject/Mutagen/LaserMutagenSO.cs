using UnityEngine;
using System.Reflection;

[CreateAssetMenu(fileName = "NewLaserData", menuName = "Combat/Laser Mutagen")]
public class LaserMutagenSO : MutagenSO
{
    [Header("Laser Properties")]
    public float laserLength = 20f;              // Length in units (20 blocks)
    public float laserWidth = 1f;                // Width of the laser beam
    public float damagePerSecond = 10f;          // Damage applied per second
    public float tickRate = 0.1f;                // How often to apply damage (in seconds)

    [Header("Visual & Sound")]
    public GameObject laserEffectPrefab;         // The visual effect for the laser
    public ShakeDataType shakeType = ShakeDataType.MeleeAttack;
    public SoundID soundEffect = SoundID.PlayerAttack;
    public AudioClip laserLoopSound;             // Optional: looping sound while active

    [Header("Behaviour")]
    public bool isDuration = true;               // If true, lasts for 'duration' seconds. If false, toggle on/off

    public override bool Activate(Player player, MutagenInstance instance)
    {
        return player.playerAttack.TriggerLaser(this, instance);
    }

    public override void Tick(Player player, MutagenInstance instance, float deltaTime)
    {
        // Get the laser controller from the instance
        if (instance.customData is not LaserController laserController)
            return;

        // Update laser position and direction. attackDirection may be non-public; try reflection then fall back to forward.
        laserController.UpdateLaser(player.playerAttack.GetAttackDirection());

        // Apply damage on tick
        laserController.ApplyDamage(player, this, deltaTime);
    }

    public override void Deactivate(Player player, MutagenInstance instance)
    {
        // Clean up the laser effect
        if (instance.customData is LaserController laserController)
        {
            laserController.Destroy();
        }
    }
}
