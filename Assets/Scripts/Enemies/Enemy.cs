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

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        enemyHealthSystem = GetComponent<HealthSystem>();
        sr = GetComponent<SpriteRenderer>();
        initialColor = sr.color;

        enemyHealthSystem.OnDamageTaken += EnemyHealthSystem_OnDamageTaken;
    }

    // Update is called once per frame
    void Update() {
        if (rb == null || Player.Instance == null) return;

        if (isKnockedBack) {
            knockbackTimer -= Time.deltaTime;

            if (knockbackTimer <= 0f) {
                isKnockedBack = false;
            }

            return;
        }

        Vector2 direction = (Player.Instance.transform.position - transform.position).normalized;

        if (Vector2.Distance(Player.Instance.transform.position, transform.position) > 0.5f) {
            rb.linearVelocity = direction * speed;
        }
        else {
            rb.linearVelocity = Vector2.zero;
        }

        // dopo essere stato colpito, ritorna al colore originale
        if (sr.color != initialColor) {
            sr.color = Color.Lerp(sr.color, initialColor, blinkAfterDamageTime);
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
}
