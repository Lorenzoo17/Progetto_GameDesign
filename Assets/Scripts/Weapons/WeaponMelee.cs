using NUnit.Framework;
using System;
using UnityEngine;
using System.Collections.Generic;

public class WeaponMelee : Weapon {
    
    [SerializeField] private float weaponRotationOffsetZ = 0f;
    [SerializeField] private float attackDuration = 0.1f; // velocita' dell'animazione di attacco
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

    public override void Attack(Vector2 dir) {
        float baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        CalculateNewRotationAngle(baseAngle);

        Player player = Player.Instance;

        HashSet<string> playerActivePerks = player.perkGestors.getActivePerks();

        if (playerActivePerks.Contains("Poison")) { 
            hasPoison = true;
        }
        
        float poisonDamegeValue = Player.Instance.playerStats.playerCurrentStats.getPoisonDamage();
        

        // gestione attacco con overlapcircle
        attackCentrePosition = Player.Instance.playerAttack.GetWeaponHolder().position + (Vector3)(dir.normalized * Player.Instance.playerAttack.attackCentreOffset);
        float weaponDamage = weaponBaseDamage; // poi si somma danno del player
        float weaponRange = weaponBaseRange; // poi si somma range del player
        Collider2D[] colliders = Physics2D.OverlapCircleAll(attackCentrePosition, weaponRange);

        foreach(Collider2D entity in colliders) {
            if(Utils.CombatUtility.CanDamage(Player.Instance.gameObject, entity.gameObject)) {
                if (entity.gameObject.TryGetComponent<IDamageable>(out IDamageable entityDamageable)) {

                    //danno normale
                    DamageInfo normalDamage = new DamageInfo(weaponDamage, dir, Player.Instance.gameObject, EntityType.Player);
                    entityDamageable.TakeDamage(normalDamage);


                    if (hasPoison)
                    {
                        DamageInfo poisonDamage = new DamageInfo(poisonDamegeValue, dir, Player.Instance.gameObject, EntityType.Player);
                        entityDamageable.TakeDamage(poisonDamage);
                    }

                    // Applica effetti dei perk
                    if (Player.Instance.perkGestors != null)
                    {
                        Player.Instance.perkGestors.ApplyAttackEffects(entity.gameObject);
                    }
                }
            }
        }

        // istanzio slash effect
        if (meleeAttackEffect == null) return;

        Quaternion meleeEffectRotation = Quaternion.Euler(0, 0, baseAngle);
        float attackSlashEffectOffset = 1.1f; // quanto distante istanziare lo slash effect rispetto al player
        GameObject slashEffect = Instantiate(meleeAttackEffect, Player.Instance.playerAttack.GetWeaponHolder().position + (Vector3)(dir.normalized * attackSlashEffectOffset), meleeEffectRotation);
        if (slashEffect.TryGetComponent<MeleeEffect>(out MeleeEffect meleeEffect)) {
            meleeEffect.SetDirection(dir);
        }
    }

    private void CalculateNewRotationAngle(float baseAngle) {
        
        float currentAngle = baseAngle + weaponRotationOffsetZ;

        startAngle = currentAngle;

        currentSwingRight = swingRight;
        swingRight = !swingRight;

        isAttacking = true;
        attackElapsed = 0f;
    }
    // rotazione dell'arma melee, basata su:
    // arma posta a 90 gradi rispetto alla direzione di attacco (mouse)
    // in questo modo a seguito dell'attacco compie una rotazione di 180 gradi in modo che
    // lo slash sia centrato rispetto alla direzione di attacco
    public override void HandleRotation(Transform weaponHolder, Vector2 dir) {
        float baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        float finalAngle = baseAngle + weaponRotationOffsetZ;

        if (isAttacking) {
            attackElapsed += Time.deltaTime;
            float t = attackElapsed / attackDuration;

            float swingAmount = 180f * t;
            float currentAngle;

            if (currentSwingRight)
                currentAngle = startAngle + swingAmount;
            else
                currentAngle = startAngle - swingAmount;

            weaponHolder.rotation = Quaternion.Euler(0, 0, currentAngle);

            if (t >= 1f) {
                isAttacking = false;

                weaponRotationOffsetZ += 180f;
                weaponRotationOffsetZ %= 360f;
            }
        }
        else {
            weaponHolder.rotation = Quaternion.Euler(0, 0, finalAngle);
        }
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackCentrePosition, weaponBaseRange);
    }
}
