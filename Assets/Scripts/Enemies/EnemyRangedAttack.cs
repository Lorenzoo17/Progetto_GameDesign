using UnityEngine;

public class EnemyRangedAttack : EnemyAttackBase {
    [SerializeField] private ProjectileShooter projectileShooter;
    [SerializeField] private bool useCurvedProjectile = true;

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

        if (useCurvedProjectile) {
            projectileShooter.ShootCurved(gameObject, direction, range);
        }
        else {
            projectileShooter.ShootLinear(gameObject, direction);
        }
    }
}
