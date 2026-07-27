using UnityEngine;

public class LinearProjectile : ProjectileBase
{
    private Rigidbody2D rb;
    private float speed;

    [SerializeField] private Transform shadow;
    [SerializeField] private Vector2 initialShadownOffset;
    [SerializeField] private float shadowInterpolationValue;

    [SerializeField] private LayerMask destroyOnContactLayers;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        if (shadow != null)
            shadow.localPosition = (Vector3)initialShadownOffset;
    }

    public void InitializeLinearProjectile(
        GameObject owner,
        Vector2 direction,
        float speed,
        float damage,
        float range = 5f
    )
    {
        if (owner == Player.Instance.gameObject)
        {
            damage += Player.Instance.playerStats.playerCurrentStats.GetAttack() / 2;
        }
        base.InitializeProjectile(owner, direction, damage, range); // metodo di ProjectileBase

        this.speed = speed;

        if (rb != null)
        {
            rb.linearVelocity = this.direction * speed;
        }
    }

    private void Update()
    {
        if (shadow != null)
            shadow.localPosition = Vector3.Lerp(shadow.localPosition, Vector3.zero, shadowInterpolationValue * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<ICollectible>() != null) return;

        if (((1 << other.gameObject.layer) & destroyOnContactLayers) != 0)
        {
            ProjectileDestruction();
            return;
        }

        Vector2 hitDirection = direction;
        bool hasHitSomething = TryDealDamage(other, direction);

        if (hasHitSomething)
        {
            ProjectileDestruction();
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
