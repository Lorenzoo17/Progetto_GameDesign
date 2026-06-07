using System.Collections;
using UnityEngine;

public class MouthwashPuddle : MonoBehaviour
{
    private float tickDamage;
    private float tickInterval;
    private float radius;
    private GameObject owner;

    private bool active = true;

    public void Initialize(
        float tickDamage,
        float tickInterval,
        float radius,
        float duration,
        GameObject owner)
    {
        this.tickDamage = tickDamage;
        this.tickInterval = tickInterval;
        this.radius = radius;
        this.owner = owner;

        StartCoroutine(DamageLoop());
        Destroy(gameObject, duration);
    }

    private IEnumerator DamageLoop()
    {
        WaitForSeconds wait = new WaitForSeconds(tickInterval);

        while (active)
        {
            ApplyDamage();
            yield return wait;
        }
    }

    private void ApplyDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);

        foreach (var hit in hits)
        {
            if (!Utils.CombatUtility.CanDamage(owner, hit.gameObject))
                continue;

            if (hit.TryGetComponent<IDamageable>(out IDamageable dmg))
            {
                Vector2 knockback =
                    (hit.transform.position - transform.position).normalized;

                DamageInfo info = new DamageInfo(
                    tickDamage,
                    knockback,
                    owner,
                    EntityType.Player
                );

                dmg.TakeDamage(info);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}