using UnityEngine;

public class FollowProjectile : ProjectileBase {
    private Rigidbody2D rb;
    private float speed;

    [SerializeField] private Transform shadow;
    [SerializeField] private Vector2 initialShadownOffset;
    [SerializeField] private float shadowInterpolationValue;

    [SerializeField] private LayerMask destroyOnContactLayers;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start() {
        if (shadow != null)
            shadow.localPosition = (Vector3)initialShadownOffset;
    }

    public void InitializeFollowProjectile(
        GameObject owner,
        Vector2 direction,
        float speed,
        float damage
    ) {
        if (owner == Player.Instance.gameObject) {
            damage += Player.Instance.playerStats.playerCurrentStats.getAttack() / 2;
        }
        base.InitializeProjectile(owner, direction, damage); // metodo di ProjectileBase

        this.speed = speed;
    }

    private void Update() {
        if (shadow != null)
            shadow.localPosition = Vector3.Lerp(shadow.localPosition, Vector3.zero, shadowInterpolationValue * Time.deltaTime);
    }

    private void FixedUpdate() {
        if (rb == null) return;

        Transform target = null;

        // Se il proiettile è del player, segue il nemico più vicino
        if (owner == Player.Instance.gameObject) {
            target = GetClosestEnemy();
        }
        // Se il proiettile è di un nemico, segue il player
        else {
            if (Player.Instance != null)
                target = Player.Instance.transform;
        }

        if (target != null) {
            Vector2 followDirection = ((Vector2)target.position - (Vector2)transform.position).normalized;

            // aggiorno anche la direction del ProjectileBase,
            // così il danno/knockback usa la direzione corretta
            direction = followDirection;

            rb.linearVelocity = followDirection * speed;
        }
        else {
            // Se non trova nemici, continua nella direzione iniziale
            rb.linearVelocity = direction.normalized * speed;
        }
    }
    private Transform GetClosestEnemy() {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        Transform closestEnemy = null;
        float closestDistance = Mathf.Infinity;

        foreach (Enemy enemy in enemies) {
            if (enemy == null) continue;
            if (!enemy.gameObject.activeInHierarchy) continue;

            float distance = Vector2.Distance(transform.position, enemy.transform.position);

            if (distance < closestDistance) {
                closestDistance = distance;
                closestEnemy = enemy.transform;
            }
        }

        return closestEnemy;
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.GetComponent<ICollectible>() != null) return;

        if (((1 << other.gameObject.layer) & destroyOnContactLayers) != 0) {
            ProjectileDestruction();
            return;
        }

        Vector2 hitDirection = direction;
        bool hasHitSomething = TryDealDamage(other, direction);

        if (hasHitSomething) {
            ProjectileDestruction();
        }
    }

}
