using System;
using UnityEngine;

public class ProjectileBase : MonoBehaviour
{
    protected GameObject owner;
    protected EntityType ownerType;
    protected Vector2 direction;
    protected float damage;
    [SerializeField] private GameObject destructionEffect;

    protected bool initialized;
    public event EventHandler OnBulletDestruction;

    [SerializeField] private float destructionRange = 5f; // distanza di base
    private Vector3 spawnPosition;
    public virtual void InitializeProjectile(
        GameObject owner,
        Vector2 direction,
        float damage,
        float range = 5f
    )
    {
        this.owner = owner;
        this.direction = direction.normalized;
        this.damage = damage;
        this.destructionRange = range;

        spawnPosition = transform.position;

        EntityOwner combatOwner = owner.GetComponent<EntityOwner>();
        ownerType = combatOwner != null ? combatOwner.GetEntityType : EntityType.Neutral;

        initialized = true;
    }

    private void FixedUpdate() {
        if (!initialized) return;

        Vector3 distanceFromSpawn = transform.position - spawnPosition;

        if(Vector3.Distance(spawnPosition, transform.position) >= destructionRange) {
            ProjectileDestruction();
        }
    }

    protected bool CanDamage(Collider2D other)
    {
        if (other == null) return false;
        if (other.gameObject == owner) return false;

        EntityOwner targetOwner = other.GetComponent<EntityOwner>();

        if (targetOwner == null) return true;

        bool canAttack = targetOwner.GetEntityType != ownerType;
        return canAttack;
    }

    protected bool TryDealDamage(Collider2D other, Vector2 hitDirection)
    {
        if (!CanDamage(other)) return false;

        if (other.TryGetComponent<IDamageable>(out IDamageable damageable))
        {
            DamageInfo damageInfo = new DamageInfo(damage, hitDirection, owner, ownerType);

            if (Player.Instance != null && Player.Instance.perkController != null)
            {
                damageInfo = Player.Instance.perkController.OnDealDamage(ref damageInfo);
            }

            damageable.TakeDamage(damageInfo);
            return true;
        }

        return false;
    }

    protected virtual void ProjectileDestruction()
    {
        if (destructionEffect != null)
        {
            GameObject effect = Instantiate(destructionEffect, transform.position, Quaternion.identity);
        }

        OnBulletDestruction?.Invoke(this, EventArgs.Empty);

        Destroy(gameObject);
    }
}
