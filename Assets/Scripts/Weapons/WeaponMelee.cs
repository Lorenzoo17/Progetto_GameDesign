using NUnit.Framework;
using System;
using UnityEngine;
using System.Collections.Generic;

public class WeaponMelee : Weapon
{

    [SerializeField] private float weaponRotationOffsetZ = 0f;
    [SerializeField] private float attackDuration = 0.1f;
    [SerializeField] private GameObject meleeAttackEffect;

    [SerializeField] private float weaponBaseRange = 1f;
    [SerializeField] private float weaponBaseDamage = 2f;

    [SerializeField] private Boolean hasPoison = false;
    [SerializeField] private float poisonDamage = 0f;

    private Vector2 attackCentrePosition;
    private bool isAttacking = false;
    private float attackElapsed = 0f;
    private float startAngle;
    private bool swingRight = true;
    private bool currentSwingRight;

    public override void Attack(Vector2 dir)
    {
        float baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        CalculateNewRotationAngle(baseAngle);

        Player player = Player.Instance;

        if (Player.Instance.playerStats.GetActivePerks().Exists(p => p.perkName == "Poison"))
        {
            hasPoison = true;
        }

        float poisonDamegeValue = Player.Instance.playerStats.playerCurrentStats.getPoisonDamage();

        attackCentrePosition = Player.Instance.playerAttack.GetWeaponHolder().position + (Vector3)(dir.normalized * Player.Instance.playerAttack.attackCentreOffset);
        float weaponDamage = weaponBaseDamage;
        float weaponRange = weaponBaseRange;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(attackCentrePosition, weaponRange);

        foreach (Collider2D entity in colliders)
        {
            if (Utils.CombatUtility.CanDamage(Player.Instance.gameObject, entity.gameObject))
            {
                if (entity.gameObject.TryGetComponent<IDamageable>(out IDamageable entityDamageable))
                {

                    // 🔥 APPLICA PERK AL DANNO NORMALE
                    DamageInfo normalDamage = new DamageInfo(weaponDamage, dir, Player.Instance.gameObject, EntityType.Player);
                    normalDamage = Player.Instance.perkController.OnDealDamage(ref normalDamage);
                    entityDamageable.TakeDamage(normalDamage);

                    // 🔥 APPLICA PERK AL DANNO DA VELENO
                    if (hasPoison)
                    {
                        DamageInfo poisonDamageInfo = new DamageInfo(poisonDamegeValue, dir, Player.Instance.gameObject, EntityType.Player);
                        poisonDamageInfo = Player.Instance.perkController.OnDealDamage(ref poisonDamageInfo);
                        entityDamageable.TakePoisonDamage(poisonDamageInfo);
                    }
                }
            }
        }

        // istanzio slash effect
        if (meleeAttackEffect == null) return;

        Quaternion meleeEffectRotation = Quaternion.Euler(0, 0, baseAngle);
        float attackSlashEffectOffset = 1.1f;
        GameObject slashEffect = Instantiate(meleeAttackEffect, Player.Instance.playerAttack.GetWeaponHolder().position + (Vector3)(dir.normalized * attackSlashEffectOffset), meleeEffectRotation);
        if (slashEffect.TryGetComponent<MeleeEffect>(out MeleeEffect meleeEffect))
        {
            meleeEffect.SetDirection(dir);
        }
    }

    private void CalculateNewRotationAngle(float baseAngle)
    {
        float currentAngle = baseAngle + weaponRotationOffsetZ;
        startAngle = currentAngle;
        currentSwingRight = swingRight;
        swingRight = !swingRight;
        isAttacking = true;
        attackElapsed = 0f;
    }

    public override void HandleRotation(Transform weaponHolder, Vector2 dir)
    {
        float baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        float finalAngle = baseAngle + weaponRotationOffsetZ;

        if (isAttacking)
        {
            attackElapsed += Time.deltaTime;
            float t = attackElapsed / attackDuration;
            float swingAmount = 180f * t;
            float currentAngle;

            if (currentSwingRight)
                currentAngle = startAngle + swingAmount;
            else
                currentAngle = startAngle - swingAmount;

            weaponHolder.rotation = Quaternion.Euler(0, 0, currentAngle);

            if (t >= 1f)
            {
                isAttacking = false;
                weaponRotationOffsetZ += 180f;
                weaponRotationOffsetZ %= 360f;
            }
        }
        else
        {
            weaponHolder.rotation = Quaternion.Euler(0, 0, finalAngle);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackCentrePosition, weaponBaseRange);
    }
}
