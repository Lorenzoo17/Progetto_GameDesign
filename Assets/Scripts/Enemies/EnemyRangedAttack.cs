using UnityEngine;

public class EnemyRangedAttack : EnemyAttackBase {
    [SerializeField] private ProjectileShooter projectileShooter;
    [SerializeField] private ShooterType shooterType;

    protected override void Awake() {
        base.Awake();

        if (projectileShooter == null) {
            projectileShooter = GetComponentInChildren<ProjectileShooter>();
        }
    }

    protected override void ExecuteAttack() {
        if (Player.Instance == null || projectileShooter == null) {
            EndAttack();
            return;
        }

        Vector2 direction = ((Vector2)Player.Instance.transform.position - (Vector2)transform.position).normalized;
        float range = Vector2.Distance(transform.position, Player.Instance.transform.position);

        int projectileNumber = 0;
        switch (shooterType) {
            case ShooterType.Linear:
                projectileShooter.ShootLinear(gameObject, direction);
                break;
            case ShooterType.Curved:
                projectileShooter.ShootCurved(gameObject, direction, range);
                break;
            case ShooterType.Circle:
                projectileNumber = Random.Range(3, 5);
                projectileShooter.ShootMultipleProjectile(gameObject, projectileNumber);
                break;
            case ShooterType.Spread:
                projectileNumber = Random.Range(2, 4);
                projectileShooter.ShootFocusedSpread(gameObject, projectileNumber, Player.Instance.transform);
                break;
            case ShooterType.Follow:
                projectileShooter.ShootFollow(gameObject, direction);
                break;
            default:
                projectileShooter.ShootLinear(gameObject, direction);
                break;
        }
        // suono di attacco ranged 
        if (SoundManager.Instance != null) {
            SoundManager.Instance.PlaySound2D(SoundID.EnemyShoot, .2f);
        }
    }
}
