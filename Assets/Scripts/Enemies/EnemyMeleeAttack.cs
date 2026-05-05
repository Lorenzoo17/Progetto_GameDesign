using FirstGearGames.SmoothCameraShaker;
using UnityEngine;

public class EnemyMeleeAttack : EnemyAttackBase {
    [SerializeField] private float attackDamage = 1f;
    [SerializeField] private float attackRange = 1f;

    protected override void ExecuteAttack() {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, attackRange);

        foreach (Collider2D entity in hitColliders) {
            if (entity == null) continue;
            if (entity.GetComponent<Enemy>() != null) continue;

            if (entity.TryGetComponent<IDamageable>(out IDamageable damageable)) {
                Vector2 direction = ((Vector2)entity.transform.position - (Vector2)transform.position).normalized;

                damageable.TakeDamage(new DamageInfo(
                    attackDamage,
                    direction,
                    gameObject,
                    EntityType.Enemy
                ));

                if (EffectManager.Instance != null) {
                    ShakeData cameraShakeAttackData = EffectManager.Instance.GetShakeDataByType(ShakeDataType.MeleeAttack);

                    if (cameraShakeAttackData != null) {
                        CameraShakerHandler.Shake(cameraShakeAttackData);
                    }
                }
            }
        }
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
