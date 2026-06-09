using UnityEngine;

[CreateAssetMenu(fileName = "GiantProjectileMutagen", menuName = "Mutagens/Giant Projectile")]
public class GiantProjectileMutagenSO : MutagenSO
{
    [Header("Projectile")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 10f;
    public float damageMultiplier = 3f;

    public override bool Activate(Player player, MutagenInstance instance)
    {
        if (projectilePrefab == null)
            return false;

        // Direzione di mira del player
        Vector2 direction = player.playerAttack.GetAttackDirection();

        // Se il player non sta mirando da nessuna parte
        if (direction == Vector2.zero)
            return false;

        // Spawn del proiettile
        GameObject projectile = Instantiate(
            projectilePrefab,
            player.transform.position,
            Quaternion.identity
        );

        // Danno basato sulle statistiche del player
        float damage = player.playerStats.playerCurrentStats.GetAttack()
                       * damageMultiplier;

        GiantProjectile giantProjectile =
            projectile.GetComponent<GiantProjectile>();

        if (giantProjectile != null)
        {
            giantProjectile.Initialize(
                direction,
                projectileSpeed,
                damage,
                player.gameObject
            );
        }

        return true;
    }

    public override void Tick(Player player, MutagenInstance instance, float deltaTime)
    {
    }

    public override void Deactivate(Player player, MutagenInstance instance)
    {
    }

    public override string Description()
    {
        return $"Shoot a giant projectile in the direction you're facing, dealing {damageMultiplier}x your base damage. The projectile travels at {projectileSpeed} units/second.";
    }
}