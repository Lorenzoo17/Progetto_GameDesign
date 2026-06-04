using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WeaponSpear : Weapon
{
    [SerializeField] private float spearLength = 4f;
    [SerializeField] private float spearWidth = 0.4f;
    [SerializeField] private float weaponBaseDamage = 2.5f;
    [SerializeField] private GameObject spearAttackEffect;
    [SerializeField] private float weaponRotationOffsetZ = 0f;

    [Header("Thrust Animation")]
    [SerializeField] private float thrustDistance = 0.5f; // Quanto avanza
    [SerializeField] private float thrustForwardDuration = 0.15f; // Tempo di spinta
    [SerializeField] private float thrustBackDuration = 0.1f; // Tempo di ritorno
    [SerializeField] private AnimationCurve thrustForwardCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve thrustBackCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Vector2 attackCentrePosition;
    private HashSet<GameObject> hitEnemiesThisAttack = new();

    public override void Attack(Vector2 dir)
    {
        hitEnemiesThisAttack.Clear();

        // 🔥 AVVIA L'ANIMAZIONE DI THRUST
        StartCoroutine(SpearThrustAnimation(dir));

        // Usa RaycastAll per colpire in linea retta (perforante)
        Vector2 rayOrigin = Player.Instance.playerAttack.GetWeaponHolder().position;
        RaycastHit2D[] hits = Physics2D.RaycastAll(
            rayOrigin,
            dir.normalized,
            spearLength
        );

        foreach (RaycastHit2D hit in hits)
        {
            if (hitEnemiesThisAttack.Contains(hit.collider.gameObject))
                continue;

            if (Utils.CombatUtility.CanDamage(Player.Instance.gameObject, hit.collider.gameObject))
            {
                if (hit.collider.TryGetComponent<IDamageable>(out IDamageable entityDamageable))
                {
                    hitEnemiesThisAttack.Add(hit.collider.gameObject);

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

    // 🔥 ANIMAZIONE THRUST
    private IEnumerator SpearThrustAnimation(Vector2 dir)
    {
        Transform weaponHolder = Player.Instance.playerAttack.GetWeaponHolder();
        Vector3 originalPosition = weaponHolder.localPosition;
        Vector3 thrustPosition = originalPosition + (Vector3)dir.normalized * thrustDistance;

        float elapsedTime = 0f;

        // Fase 1: Spinta in avanti
        while (elapsedTime < thrustForwardDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / thrustForwardDuration;
            float curveValue = thrustForwardCurve.Evaluate(progress);
            weaponHolder.localPosition = Vector3.Lerp(originalPosition, thrustPosition, curveValue);
            yield return null;
        }

        elapsedTime = 0f;

        // Fase 2: Ritorno veloce
        while (elapsedTime < thrustBackDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / thrustBackDuration;
            float curveValue = thrustBackCurve.Evaluate(progress);
            weaponHolder.localPosition = Vector3.Lerp(thrustPosition, originalPosition, curveValue);
            yield return null;
        }

        weaponHolder.localPosition = originalPosition;
    }

    public override void HandleRotation(Transform weaponHolder, Vector2 dir)
    {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        weaponHolder.rotation = Quaternion.Euler(0, 0, angle + weaponRotationOffsetZ - 90f);
    }

    private void OnDrawGizmos()
    {
        if (hitEnemiesThisAttack.Count == 0) return;

        Gizmos.color = Color.cyan;
        Vector2 rayOrigin = Player.Instance != null ? Player.Instance.playerAttack.GetWeaponHolder().position : transform.position;
        Gizmos.DrawLine(rayOrigin, rayOrigin + (Vector2.right * spearLength));
    }
}
