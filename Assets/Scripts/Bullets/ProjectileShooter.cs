using UnityEngine;

public enum ShooterType
{
    Linear,
    Curved,
    Spread,
    Circle
}
public class ProjectileShooter : MonoBehaviour
{

    [SerializeField] private GameObject projectilePrefab;
    public Transform firePoint;

    [SerializeField] private float damage = 1f;
    [SerializeField] private float projectileSpeed = 8f;
    [SerializeField] private float projectileRange = 8f;

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

    public void ShootLinear(GameObject owner, Vector2 direction)
    {
        if (projectilePrefab == null) return;

        GameObject projectileObj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        if (projectileObj.TryGetComponent<LinearProjectile>(out LinearProjectile projectile))
        {
            projectile.InitializeLinearProjectile(owner, direction, projectileSpeed, damage);
        }
    }

    public void ShootCurved(GameObject owner, Vector2 direction, float range = -1f)
    {
        if (projectilePrefab == null) return;

        float finalRange = range > 0f ? range : projectileRange;

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
                damage
            );
        }
    }

    public void ShootMultipleProjectile(GameObject owner, int projectileNumber, Transform target = null, bool circle = true)
    {
        if (projectileNumber <= 1) return;

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
                projectile.InitializeLinearProjectile(owner, direction, projectileSpeed, damage);
            }
        }
    }

    public void ShootFocusedSpread(GameObject owner, int projectileNumber, Transform target, float spreadAngle = 45f)
    {
        if (projectileNumber <= 0) return;
        if (target == null) return;

        Vector2 targetDirection =
            ((Vector2)target.position - (Vector2)firePoint.position).normalized;

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
                    damage
                );
            }
        }
    }

    public float GetDamage()
    {
        return damage;
    }
}
