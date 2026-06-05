using UnityEngine;

public class WeaponRanged : Weapon
{
    [SerializeField] private float weaponRotationOffsetZ;
    [SerializeField] private ProjectileShooter projectileShooter;

    [SerializeField] private ShooterType shootingType;

    [Header("Settings in base a tipo di shooter")]
    [SerializeField] private float curvedProjectileRange;
    [SerializeField] private int projectileNumber;

    private void Awake() {
        if (projectileShooter == null) {
            projectileShooter = GetComponentInChildren<ProjectileShooter>();
        }
    }

    public override void Attack(Vector2 dir) {
        if (projectileShooter == null) {
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
    }

    public override void HandleRotation(Transform weaponHolder, Vector2 dir) {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        Quaternion weaponHolderRotation = Quaternion.Euler(0, 0, angle + weaponRotationOffsetZ);
        weaponHolder.rotation = weaponHolderRotation;
    }
}
