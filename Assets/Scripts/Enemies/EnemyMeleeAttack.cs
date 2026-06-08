using FirstGearGames.SmoothCameraShaker;
using UnityEngine;

public class EnemyMeleeAttack : EnemyAttackBase {
    [SerializeField] private float attackDamage = 1f;
    [SerializeField] private float attackRange = 1f;

    private Vector2 attackPoint;
    protected override void ExecuteAttack() {
        // al momento si pone il punto di attacco tra il nemico e il player (in quella direzione)
        attackPoint = transform.position;

        if (Player.Instance != null) {
            Vector2 directionToPlayer = (Player.Instance.transform.position - transform.position).normalized;
            attackPoint = (Vector2)transform.position + directionToPlayer * (attackRange * 0.5f);
        }

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(attackPoint, attackRange);

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

                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlaySound2D(SoundID.EnemySmash, .2f);
                }
            }
        }
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint, attackRange);
    }
}
