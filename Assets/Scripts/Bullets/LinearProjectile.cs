using UnityEngine;

public class LinearProjectile : ProjectileBase {
    private Rigidbody2D rb;
    private float speed;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
    }

    public void InitializeLinearProjectile(
        GameObject owner,
        Vector2 direction,
        float speed,
        float damage
    ) {
        base.InitializeProjectile(owner, direction, damage); // metodo di ProjectileBase

        this.speed = speed;

        if (rb != null) {
            rb.linearVelocity = this.direction * speed;
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.GetComponent<ICollectible>() != null) return;

        Vector2 hitDirection = direction;
        bool hasHitSomething = TryDealDamage(other, direction);

        if (hasHitSomething) {
            Destroy(gameObject);
        }
    }

    /*
 * per distruzione toccando muri
 private void OnTriggerEnter2D(Collider2D other) {
if (other.GetComponent<ICollectible>() != null) return;

if (((1 << other.gameObject.layer) & destroyOnContactLayers) != 0) {
    Destroy(gameObject);
    return;
}

bool hasHitSomething = TryDealDamage(other, direction);

if (hasHitSomething) {
    Destroy(gameObject);
}
}
 */
}
