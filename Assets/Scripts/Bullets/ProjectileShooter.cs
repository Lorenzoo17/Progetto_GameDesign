using UnityEngine;

public class ProjectileShooter : MonoBehaviour {
    
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    
    [SerializeField] private float damage = 1f;
    [SerializeField] private float projectileSpeed = 8f;
    [SerializeField] private float projectileRange = 8f;
    
    [SerializeField] private float projectileMaxHeight = 1f;
    [SerializeField] private AnimationCurve trajectoryAnimationCurve;
    [SerializeField] private AnimationCurve axisCorrectionAnimationCurve;
    [SerializeField] private AnimationCurve projectileSpeedAnimationCurve;

    private void Awake() {
        if (firePoint == null) {
            firePoint = transform;
        }
    }

    public void ShootLinear(GameObject owner, Vector2 direction) {
        if (projectilePrefab == null) return;

        GameObject projectileObj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        if (projectileObj.TryGetComponent<LinearProjectile>(out LinearProjectile projectile)) {
            projectile.InitializeLinearProjectile(owner, direction, projectileSpeed, damage);
        }
    }

    public void ShootCurved(GameObject owner, Vector2 direction, float range = -1f) {
        if (projectilePrefab == null) return;

        float finalRange = range > 0f ? range : projectileRange;

        GameObject projectileObj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        if (projectileObj.TryGetComponent<CurvedProjectile>(out CurvedProjectile projectile)) {
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

    public void ShootMultipleProjectile(GameObject owner, int projectileNumber) {
        if (projectileNumber <= 1) return;

        float angleStep = 360f / projectileNumber;

        for (int i = 0; i < projectileNumber; i++) {
            float angle = angleStep * i * Mathf.Deg2Rad;

            GameObject projectileObj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            if (projectileObj.TryGetComponent<LinearProjectile>(out LinearProjectile projectile)) {
                projectile.InitializeLinearProjectile(owner, direction, projectileSpeed, damage);
            }
        }
    }
}
