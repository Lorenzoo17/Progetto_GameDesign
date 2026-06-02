using UnityEngine;

public class WeaponRanged : Weapon
{
    [SerializeField] private float weaponRotationOffsetZ;
    [SerializeField] private ProjectileShooter projectileShooter;

    [SerializeField] private bool curvedProjectile;
    [SerializeField] private float curvedProjectileRange;

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

        if (curvedProjectile) {
            projectileShooter.ShootCurved(Player.Instance.gameObject, dir, curvedProjectileRange);
        }
        else {
            projectileShooter.ShootLinear(Player.Instance.gameObject, dir);
        }

        // suono di sparo
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound2D(SoundID.PlayerShoot, .15f);
        }
    }

    public override void HandleRotation(Transform weaponHolder, Vector2 dir) {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        Quaternion weaponHolderRotation = Quaternion.Euler(0, 0, angle + weaponRotationOffsetZ);
        weaponHolder.rotation = weaponHolderRotation;
    }
}
