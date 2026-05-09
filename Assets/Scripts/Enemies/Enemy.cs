using FirstGearGames.SmoothCameraShaker;
using UnityEngine;

public class Enemy : MonoBehaviour {

    [SerializeField] private float knockBackForce;
    [SerializeField] private float knockbackDuration = 0.2f;

    [SerializeField] private Color blinkAfterDamageTargetColor;
    [SerializeField] private float blinkAfterDamageTime;
    [SerializeField] private GameObject hitEffect;
    [SerializeField] private float hitEffectSpawnPositionOffset = 0.5f;
    [SerializeField] private float hitEffectRotationOffset = -90f;

    [SerializeField] private float enemyTouchDamage = 1f; // danno a contatto fatto al player

    private HealthSystem enemyHealthSystem;
    private EnemyMovement enemyMovement;

    private SpriteRenderer sr;
    private Color initialColor;

    private Animator anim;

    private void Awake() {
        enemyHealthSystem = GetComponent<HealthSystem>();
        enemyMovement = GetComponent<EnemyMovement>();

        sr = GetComponent<SpriteRenderer>();
        initialColor = sr.color;

        anim = GetComponent<Animator>();

        enemyHealthSystem.OnDamageTaken += EnemyHealthSystem_OnDamageTaken;
    }

    private void Update() {
        if (sr != null && sr.color != initialColor) {
            sr.color = Color.Lerp(sr.color, initialColor, blinkAfterDamageTime * Time.deltaTime);
        }

        FlipBasedOnPlayer();
    }

    private void EnemyHealthSystem_OnDamageTaken(object sender, DamageEventArgs e) {
        if (enemyMovement != null) {
            enemyMovement.ApplyKnockback(
                e.AttackDirection,
                knockBackForce,
                knockbackDuration
            );
        }
        else {
            if(TryGetComponent<EnemyMovementNav>(out EnemyMovementNav nav)) {
                nav.ApplyKnockback(
                    e.AttackDirection,
                    knockBackForce,
                    knockbackDuration
                );
            }
        }

        if (anim != null) {
            anim.SetTrigger("Hurt");
        }

        if (sr != null) {
            sr.color = blinkAfterDamageTargetColor * 3f;
        }

        if (hitEffect != null) {
            Vector2 spawnPos = (Vector2)transform.position + e.AttackDirection.normalized * hitEffectSpawnPositionOffset;
            float angle = Mathf.Atan2(e.AttackDirection.y, e.AttackDirection.x) * Mathf.Rad2Deg;

            GameObject effect = Instantiate(hitEffect, spawnPos, Quaternion.identity);
            effect.transform.rotation = Quaternion.Euler(0f, 0f, angle + hitEffectRotationOffset);
        }

        if (enemyHealthSystem.CurrentHealth <= 0) {
            DeadManagement();
        }
    }

    // a contatto con il player il player riceve danno
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.TryGetComponent<Player>(out Player player)) {
            Vector2 attackDirection = (other.transform.position - transform.position).normalized;
            player.gameObject.GetComponent<IDamageable>().TakeDamage(new DamageInfo(enemyTouchDamage, attackDirection, this.gameObject, EntityType.Enemy));
        }
    }

    private void FlipBasedOnPlayer() {
        if (Player.Instance == null) return;

        if(Player.Instance.transform.position.x > transform.position.x) {
            sr.flipX = true;
        }
        else {
            sr.flipX = false;
        }
    }

    private void DeadManagement() {
        Destroy(gameObject);
    }
}
