using UnityEngine;

public class WeaponRanged : Weapon
{
    [SerializeField] private float weaponRotationOffsetZ;
    [SerializeField] private ProjectileShooter projectileShooter;

    [SerializeField] private ShooterType shootingType;

    [Header("Settings in base a tipo di shooter")]
    [SerializeField] private float curvedProjectileRange;
    [SerializeField] private int projectileNumber;

    private void Awake()
    {
        if (projectileShooter == null)
        {
            projectileShooter = GetComponentInChildren<ProjectileShooter>();
        }
    }

    public override void Attack(Vector2 dir)
    {
        if (projectileShooter == null)
        {
            Debug.LogWarning($"ProjectileShooter non assegnato su {gameObject.name}");
            return;
        }

        switch (shootingType) {
            case ShooterType.Curved:
                projectileShooter.ShootCurved(Player.Instance.gameObject, dir, curvedProjectileRange);
                break;
            case ShooterType.Linear:
                projectileShooter.ShootLinear(Player.Instance.gameObject, dir);
                break;
            case ShooterType.Spread:
                projectileShooter.ShootFocusedSpread(Player.Instance.gameObject, projectileNumber, dir);
                break;
            default:
                projectileShooter.ShootLinear(Player.Instance.gameObject, dir);
                break;
        }

        // suono di sparo
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound2D(SoundID.PlayerShoot, .15f);
        }
    }

    public override void HandleRotation(Transform weaponHolder, Vector2 dir)
    {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        Quaternion weaponHolderRotation = Quaternion.Euler(0, 0, angle + weaponRotationOffsetZ);
        weaponHolder.rotation = weaponHolderRotation;
    }
    public override string Description() {
        if (projectileShooter == null)
            return "Ranged weapon without assigned ProjectileShooter.";

        string description = shootingType switch {
            ShooterType.Curved =>
                $"Curved ranged weapon with base damage {projectileShooter.GetDamage()} and curved range {curvedProjectileRange}.",

            ShooterType.Linear =>
                $"Linear ranged weapon with base damage {projectileShooter.GetDamage()}.",

            ShooterType.Spread =>
                $"Spread ranged weapon with base damage {projectileShooter.GetDamage()} and {projectileNumber} projectiles.",

            _ =>
                $"Ranged weapon with base damage {projectileShooter.GetDamage()}."
        };

        return description;
    }
}
