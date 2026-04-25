using FirstGearGames.SmoothCameraShaker;
using UnityEngine;

public class Enemy : MonoBehaviour {

    // semplice logica di follow per prova
    [SerializeField] private float knockBackForce;
    [SerializeField] private float speed = 2f;
    private Rigidbody2D rb;

    private HealthSystem enemyHealthSystem;

    private bool isKnockedBack;
    private float knockbackTimer;
    [SerializeField] private float knockbackDuration = 0.2f;

    private SpriteRenderer sr;
    private Color initialColor;
    [SerializeField] private Color blinkAfterDamageTargetColor;
    [SerializeField] private float blinkAfterDamageTime;
    [SerializeField] private GameObject hitEffect;
    [SerializeField] private float hitEffectSpawnPositionOffset = 0.5f;
    [SerializeField] private float hitEffectRotationOffset = -90f;

    [SerializeField] private float attackDamage;
    [SerializeField] private float attackRange = 1f; // range di zona di attacco rispetto a centro dell'enemy
    [SerializeField] private float attackDistance = 1f; // quando iniziare ad attaccare rispetto alla distanza dal player
    [SerializeField] private float attackDuration = 0.4f;
    [SerializeField] private float attackCooldown = 1f;

    private bool isAttacking = false;
    private float attackTimer = 0f;
    private float attackCooldownTimer = 0f;

    [SerializeField] private ShakeData cameraShakeAttackData;
    [SerializeField] private float attackHitDelay = 0.15f;
    private bool hasDealtDamage;

    private Animator anim;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        enemyHealthSystem = GetComponent<HealthSystem>();
        sr = GetComponent<SpriteRenderer>();
        initialColor = sr.color;

        anim = GetComponent<Animator>();

        enemyHealthSystem.OnDamageTaken += EnemyHealthSystem_OnDamageTaken;
    }

    // Update is called once per frame
    void Update() {
        if (rb == null || Player.Instance == null) return;

        // Dopo essere stato colpito ritorna al colore originale
        if (sr.color != initialColor) {
            sr.color = Color.Lerp(sr.color, initialColor, blinkAfterDamageTime);
        }

        if (isKnockedBack) {
            knockbackTimer -= Time.deltaTime;

            if (knockbackTimer <= 0f) {
                isKnockedBack = false;
            }

            return;
        }

        if (isAttacking) {
            attackTimer -= Time.deltaTime;

            // momento in cui parte effettivamente il colpo
            if (!hasDealtDamage && attackTimer <= attackDuration - attackHitDelay) {
                DealDamage();
                hasDealtDamage = true;
            }

            if (attackTimer <= 0f) {
                isAttacking = false;
            }

            rb.linearVelocity = Vector2.zero;
            return;
        }

        // cooldown attacco
        if (attackCooldownTimer > 0f) {
            attackCooldownTimer -= Time.deltaTime;
        }

        Vector2 direction = (Player.Instance.transform.position - transform.position).normalized;
        float distance = Vector2.Distance(Player.Instance.transform.position, transform.position);

        if (distance <= attackDistance && attackCooldownTimer <= 0f) {
            Attack();
            return;
        }

        if (distance > attackDistance) {
            rb.linearVelocity = direction * speed;
        }
        else {
            rb.linearVelocity = Vector2.zero;
        }

        anim.SetBool("Moving", rb.linearVelocity != Vector2.zero);
    }

    private void Attack() {
        isAttacking = true;
        attackTimer = attackDuration;
        attackCooldownTimer = attackCooldown;

        hasDealtDamage = false;

        rb.linearVelocity = Vector2.zero;

        anim.SetTrigger("Attack");
    }

    private void DealDamage() {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, attackRange);

        foreach (Collider2D entity in hitColliders) {
            if (entity != null && entity.gameObject != this.gameObject) {
                if (entity.TryGetComponent<IDamageable>(out IDamageable entityDamageable)) {
                    Vector2 attackDirection = (entity.transform.position - transform.position).normalized;
                    entityDamageable.TakeDamage(attackDamage, attackDirection);

                    // per ora per ogni entity, poi andra' fatto solo per una
                    if (cameraShakeAttackData != null) {
                        CameraShakerHandler.Shake(cameraShakeAttackData);
                    }
                }
            }
        }
    }

    private void DeadManagement() {
        // cambio colore
        // effetto di danno

        // backlash ancora piu' forte -> si spawna prefab solo visivo e si fa ruotare tanto 
        // con spawn di macchie di sangue in terra ogni tot
        Destroy(gameObject);
    }

    private void EnemyHealthSystem_OnDamageTaken(object sender, DamageEventArgs e) {

        if (rb != null) {
            rb.linearVelocity = Vector2.zero; // reset

            rb.AddForce(e.AttackDirection.normalized * knockBackForce, ForceMode2D.Impulse);

            isKnockedBack = true;
            knockbackTimer = knockbackDuration;
        }

        anim.SetTrigger("Hurt");

        // cambio colore dopo hit
        sr.color = blinkAfterDamageTargetColor * 3f; // * 3 in quanto (se compatibile con URP) cosi diventa "emissivo" e quindi effettivamente bianco ad esempio
        // spawn effetto particellare dopo hit
        if(hitEffect != null) {
            Vector2 spawnPos = (Vector2)transform.position + e.AttackDirection.normalized * hitEffectSpawnPositionOffset;
            float angle = Mathf.Atan2(e.AttackDirection.y, e.AttackDirection.x) * Mathf.Rad2Deg;
            
            GameObject effect = Instantiate(hitEffect, spawnPos, Quaternion.identity);
            effect.transform.rotation = Quaternion.Euler(0f, 0f, angle + hitEffectRotationOffset);
        }

        if (enemyHealthSystem.CurrentHealth <= 0) {
            DeadManagement();
        }
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
