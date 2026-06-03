using UnityEngine;
using System.Collections.Generic;

public class WeaponSpear : Weapon
{

    [SerializeField] private float spearLength = 4f; // lunghezza della lancia
    [SerializeField] private float spearWidth = 0.4f; // larghezza della punta
    [SerializeField] private float weaponBaseDamage = 2.5f;
    [SerializeField] private GameObject spearAttackEffect;
    [SerializeField] private float weaponRotationOffsetZ = 0f;

    private Vector2 attackCentrePosition;
    private HashSet<GameObject> hitEnemiesThisAttack = new(); // Traccia nemici già colpiti per evitare multi-hit

    public override void Attack(Vector2 dir)
    {
        hitEnemiesThisAttack.Clear();

        // Usa RaycastAll per colpire in linea retta (perforante)
        Vector2 rayOrigin = Player.Instance.playerAttack.GetWeaponHolder().position;
        RaycastHit2D[] hits = Physics2D.RaycastAll(
            rayOrigin,
            dir.normalized,
            spearLength
        );

        foreach (RaycastHit2D hit in hits)
        {
            // Evita di colpire lo stesso nemico due volte
            if (hitEnemiesThisAttack.Contains(hit.collider.gameObject))
                continue;

            if (Utils.CombatUtility.CanDamage(Player.Instance.gameObject, hit.collider.gameObject))
            {
                if (hit.collider.TryGetComponent<IDamageable>(out IDamageable entityDamageable))
                {
                    hitEnemiesThisAttack.Add(hit.collider.gameObject);

                    // 🔥 APPLICA PERK AL DANNO
                    DamageInfo damageInfo = new DamageInfo(weaponBaseDamage, dir, Player.Instance.gameObject, EntityType.Player);
                    damageInfo = Player.Instance.perkController.OnDealDamage(ref damageInfo);
                    entityDamageable.TakeDamage(damageInfo);
                }
            }
        }

        // Effetto visivo lancia
        if (spearAttackEffect != null)
        {
            attackCentrePosition = rayOrigin + (dir.normalized * (spearLength / 2f));
            Quaternion effectRotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
            GameObject effect = Instantiate(spearAttackEffect, (Vector3)attackCentrePosition, effectRotation);
            Destroy(effect, 0.6f);
        }
    }

    public override void HandleRotation(Transform weaponHolder, Vector2 dir)
    {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        weaponHolder.rotation = Quaternion.Euler(0, 0, angle + weaponRotationOffsetZ);
    }

    private void OnDrawGizmos()
    {
        if (hitEnemiesThisAttack.Count == 0) return;

        // Disegna il raggio di attacco
        Gizmos.color = Color.cyan;
        Vector2 rayOrigin = Player.Instance != null ? Player.Instance.playerAttack.GetWeaponHolder().position : transform.position;
        Gizmos.DrawLine(rayOrigin, rayOrigin + (Vector2.right * spearLength)); // Disegna linea (approssimativa)
    }
}
