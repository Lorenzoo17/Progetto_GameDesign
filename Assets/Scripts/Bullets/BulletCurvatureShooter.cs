using UnityEngine;

public class BulletCurvatureShooter : MonoBehaviour {

    [SerializeField] private GameObject projectilePrefab;

    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Projectile Settings")]
    [SerializeField] private float shootRate;
    [SerializeField] private float projectileMaxMoveSpeed;
    [SerializeField] private float projectileMaxHeight;
    [SerializeField] private float projectileRange = 8f;

    [Header("Curves")]
    [SerializeField] private AnimationCurve trajectoryAnimationCurve;
    [SerializeField] private AnimationCurve axisCorrectionAnimationCurve;
    [SerializeField] private AnimationCurve projectileSpeedAnimationCurve;

    private float shootTimer;

    private void Update() {
        shootTimer -= Time.deltaTime;

        if (Input.GetMouseButtonDown(0)) {
            if (shootTimer <= 0f) {
                shootTimer = shootRate;
                Shoot();
            }
        }
    }

    private void Shoot() {
        Vector2 direction;
        float finalRange;

        if (target != null) {
            direction = ((Vector2)target.position - (Vector2)transform.position).normalized;
            finalRange = Vector2.Distance(transform.position, target.position);
        }
        else {
            direction = InputManager.Instance.CalculateAimDirection(transform.position);
            finalRange = projectileRange;
        }

        BulletCurvature projectile = Instantiate(
            projectilePrefab,
            transform.position,
            Quaternion.identity
        ).GetComponent<BulletCurvature>();

        projectile.InitializeAnimationCurves(
            trajectoryAnimationCurve,
            axisCorrectionAnimationCurve,
            projectileSpeedAnimationCurve
        );

        projectile.InitializeProjectile(
            direction,
            finalRange,
            projectileMaxMoveSpeed,
            projectileMaxHeight
        );
    }
}