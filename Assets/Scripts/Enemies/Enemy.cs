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

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        enemyHealthSystem = GetComponent<HealthSystem>();

        enemyHealthSystem.OnDamageTaken += EnemyHealthSystem_OnDamageTaken;
    }

    private void EnemyHealthSystem_OnDamageTaken(object sender, DamageEventArgs e) {

        if (rb != null) {
            rb.linearVelocity = Vector2.zero; // reset

            rb.AddForce(e.AttackDirection.normalized * knockBackForce, ForceMode2D.Impulse);

            isKnockedBack = true;
            knockbackTimer = knockbackDuration;
        }

        if (enemyHealthSystem.CurrentHealth <= 0) {
            DeadManagement();
        }
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
    }

    private void DeadManagement() {
        // cambio colore
        // effetto di danno

        // backlash ancora piu' forte -> si spawna prefab solo visivo e si fa ruotare tanto 
        // con spawn di macchie di sangue in terra ogni tot
        Destroy(gameObject);
    }
}
