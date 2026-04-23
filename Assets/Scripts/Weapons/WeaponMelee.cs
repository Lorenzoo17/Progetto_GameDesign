using UnityEngine;

public class WeaponMelee : Weapon {
    
    [SerializeField] private float weaponRotationOffsetZ = 0f;
    [SerializeField] private float attackDuration = 0.1f; // velocita' dell'animazione di attacco
    [SerializeField] private GameObject meleeAttackEffect;

    private bool isAttacking = false;
    private float attackElapsed = 0f;

    private float startAngle;
    private bool swingRight = true;
    private bool currentSwingRight;

    public override void Attack(Vector2 dir) {
        float baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        CalculateNewRotationAngle(baseAngle);

        // gestione attacco con overlapcircle

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
}
