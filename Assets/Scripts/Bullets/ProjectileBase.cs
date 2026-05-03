using UnityEngine;

public class ProjectileBase : MonoBehaviour {
    protected GameObject owner;
    protected EntityType ownerType;
    protected Vector2 direction;
    protected float damage;

    protected bool initialized;

    public virtual void InitializeProjectile(
        GameObject owner,
        Vector2 direction,
        float damage
    ) {
        this.owner = owner;
        this.direction = direction.normalized;
        this.damage = damage;

        EntityOwner combatOwner = owner.GetComponent<EntityOwner>();
        ownerType = combatOwner != null ? combatOwner.GetEntityType : EntityType.Neutral;

        initialized = true;
    }

    protected bool CanDamage(Collider2D other) {
        if (other == null) return false;
        if (other.gameObject == owner) return false;

        EntityOwner targetOwner = other.GetComponent<EntityOwner>();

        if (targetOwner == null) return true;

        bool canAttack = targetOwner.GetEntityType != ownerType;
        return canAttack;
    }

    protected bool TryDealDamage(Collider2D other, Vector2 hitDirection) {
        if (!CanDamage(other)) return false;

        if (other.TryGetComponent<IDamageable>(out IDamageable damageable)) {
            damageable.TakeDamage(new DamageInfo(damage, hitDirection, owner, ownerType));
            return true;
        }

        return false;
    }
}
