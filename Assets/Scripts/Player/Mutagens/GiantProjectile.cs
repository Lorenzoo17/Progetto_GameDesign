using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class GiantProjectile : MonoBehaviour
{
    private Rigidbody2D rb;

    private float damage;
    private GameObject owner;

    [SerializeField] private float lifeTime = 5f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(
        Vector2 direction,
        float speed,
        float damage,
        GameObject owner)
    {
        this.damage = damage;
        this.owner = owner;

        rb.linearVelocity = direction.normalized * speed;

        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!Utils.CombatUtility.CanDamage(owner, other.gameObject))
            return;

        if (other.TryGetComponent<IDamageable>(out IDamageable damageable))
        {
            Vector2 knockback =
                (other.transform.position - transform.position).normalized;

            DamageInfo info = new DamageInfo(
                damage,
                knockback,
                owner,
                EntityType.Player
            );

            damageable.TakeDamage(info);

            Destroy(gameObject);
        }
    }
}