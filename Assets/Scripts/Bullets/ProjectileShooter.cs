using UnityEngine;

public enum ShooterType
{
    Linear,
    Curved,
    Spread,
    Circle,
    Follow
}
public class ProjectileShooter : MonoBehaviour {
    
    [SerializeField] protected GameObject projectilePrefab;
    public Transform firePoint;
    [SerializeField] protected float projectileSpeed = 8f;
    [SerializeField] private float projectileRange = 16f;

    [SerializeField] private float projectileMaxHeight = 1f;
    [SerializeField] private AnimationCurve trajectoryAnimationCurve;
    [SerializeField] private AnimationCurve axisCorrectionAnimationCurve;
    [SerializeField] private AnimationCurve projectileSpeedAnimationCurve;

    private void Awake()
    {
        if (firePoint == null)
        {
            firePoint = transform;
        }
    }

    private float GetCurrentDamage()
    {
        WeaponRanged weapon = Player.Instance.playerAttack.GetCurrentWeapon() as WeaponRanged;
        return weapon != null ? weapon.getCurrentDamage() : 0f;
    }

    public void ShootLinear(GameObject owner, Vector2 direction, float overrideDamage = -1f)
    {
        if (projectilePrefab == null) return;

        float currentDamage = overrideDamage >= 0f ? overrideDamage : GetCurrentDamage();
        GameObject projectileObj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        if (projectileObj.TryGetComponent<LinearProjectile>(out LinearProjectile projectile))
        {
            projectile.InitializeLinearProjectile(owner, direction, projectileSpeed, currentDamage, projectileRange);
        }
    }

    public void ShootFollow(GameObject owner, Vector2 direction, float overrideDamage = -1f) {
        if (projectilePrefab == null) return;

        float currentDamage = overrideDamage >= 0f ? overrideDamage : GetCurrentDamage();
        GameObject projectileObj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        if (projectileObj.TryGetComponent<FollowProjectile>(out FollowProjectile projectile)) {
            projectile.InitializeFollowProjectile(owner, direction, projectileSpeed, currentDamage, projectileRange);
        }
    }

    public void ShootCurved(GameObject owner, Vector2 direction, float range = -1f, float overrideDamage = -1f)
    {
        if (projectilePrefab == null) return;

        float finalRange = range > 0f ? range : projectileRange;
        float currentDamage = overrideDamage >= 0f ? overrideDamage : GetCurrentDamage();

        GameObject projectileObj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        if (projectileObj.TryGetComponent<CurvedProjectile>(out CurvedProjectile projectile))
        {
            projectile.InitializeAnimationCurves(
                trajectoryAnimationCurve,
                axisCorrectionAnimationCurve,
                projectileSpeedAnimationCurve
            );

            projectile.InitializeProjectile(
                owner,
                direction,
                finalRange,
                projectileSpeed,
                projectileMaxHeight,
                currentDamage
            );
        }
    }

    public void ShootMultipleProjectile(GameObject owner, int projectileNumber, Transform target = null, bool circle = true, float overrideDamage = -1f)
    {
        if (projectileNumber <= 1) return;

        float currentDamage = overrideDamage >= 0f ? overrideDamage : GetCurrentDamage();
        float targetAngle = 0f;

        if (target != null)
        {
            Vector2 targetDirection = ((Vector2)(target.position - firePoint.position).normalized);
            targetAngle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;
        }


        float arcAngle = circle ? 360f : 120f;
        float angleStep = circle ?
            arcAngle / projectileNumber
            : projectileNumber > 1 ? arcAngle / (projectileNumber - 1) : 0f;

        float startAngle = circle
            ? targetAngle
            : targetAngle - arcAngle / 2f;

        for (int i = 0; i < projectileNumber; i++)
        {
            float angle = (startAngle + angleStep * i) * Mathf.Deg2Rad;

            GameObject projectileObj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            if (projectileObj.TryGetComponent<LinearProjectile>(out LinearProjectile projectile))
            {
                projectile.InitializeLinearProjectile(owner, direction, projectileSpeed, currentDamage, projectileRange);
            }
        }
    }

    public void ShootFocusedSpread(GameObject owner, int projectileNumber, Transform target, float spreadAngle = 45f, float overrideDamage = -1f)
    {
        if (projectileNumber <= 0) return;
        if (target == null) return;

        Vector2 targetDirection =
            ((Vector2)target.position - (Vector2)firePoint.position).normalized;

        ShootSpread(owner, projectileNumber, targetDirection, spreadAngle, overrideDamage);
    }

    public void ShootFocusedSpread(GameObject owner, int projectileNumber, Vector2 direction, float spreadAngle = 45f, float overrideDamage = -1f) {
        if (projectileNumber <= 0) return;
        if (direction == null) return;

        Vector2 targetDirection = direction;

        ShootSpread(owner, projectileNumber, targetDirection, spreadAngle, overrideDamage);
    }

    private void ShootSpread(GameObject owner, int projectileNumber, Vector2 targetDirection, float spreadAngle, float overrideDamage = -1f) {
        float currentDamage = overrideDamage >= 0f ? overrideDamage : GetCurrentDamage();
        float targetAngle = Mathf.Atan2(
            targetDirection.y,
            targetDirection.x
        ) * Mathf.Rad2Deg;

        float angleStep = projectileNumber > 1
            ? spreadAngle / (projectileNumber - 1)
            : 0f;

        float startAngle = targetAngle - spreadAngle / 2f;

        for (int i = 0; i < projectileNumber; i++)
        {
            float angleDeg = startAngle + angleStep * i;
            float angleRad = angleDeg * Mathf.Deg2Rad;

            Vector2 direction = new Vector2(
                Mathf.Cos(angleRad),
                Mathf.Sin(angleRad)
            ).normalized;

            GameObject projectileObj = Instantiate(
                projectilePrefab,
                firePoint.position,
                Quaternion.identity
            );

            if (projectileObj.TryGetComponent<LinearProjectile>(out LinearProjectile projectile))
            {
                projectile.InitializeLinearProjectile(
                    owner,
                    direction,
                    projectileSpeed,
                    currentDamage,
                    projectileRange
                );
            }
        }
    }
}
