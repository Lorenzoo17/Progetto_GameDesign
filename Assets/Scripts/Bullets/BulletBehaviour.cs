using UnityEngine;

public class BulletBehaviour : MonoBehaviour {

    private Rigidbody2D rb;

    private Vector2 direction;
    private float damageToDeal;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetBullet(Vector2 direction, float speed, float damage) {
        this.direction = direction;
        this.damageToDeal = damage;

        rb.linearVelocity = direction * speed;
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.GetComponent<ICollectible>() != null) return;

        if(other.TryGetComponent<IDamageable>(out IDamageable entity)) {
            entity.TakeDamage(damageToDeal, direction);
        }

        // effetto di distruzione
        Destroy(gameObject);
    }
}
