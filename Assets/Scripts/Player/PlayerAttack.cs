using System;
using UnityEngine;

public class PlayerAttack : MonoBehaviour {

    [SerializeField] private Transform weaponHolder;
    [SerializeField] private float weaponRotationOffsetZ = 0f;

    private bool canAttack = true;
    private float attackTimer = 0f;

    public GameObject currentWeapon;
    private GameObject currentWeaponInstance;

    private bool isAttacking = false;
    private float attackDuration = 0.1f;
    private float attackElapsed = 0f;

    private float startAngle;
    private bool swingRight = true;
    private bool currentSwingRight;

    private Vector2 attackCentre;
    [SerializeField] private float distanceFromPlayer;
    [SerializeField] private GameObject meleeAttackEffect;

    private void Start() {
        InputManager.Instance.OnAttackEvent += Attack;
    }

    private void Update() {
        HandleWeaponRotation();

        if (!canAttack) {
            attackTimer -= Time.deltaTime;

            if (attackTimer <= 0f) {
                canAttack = true;
            }
        }
    }

    // ----------- GESTIONE ATTACCO ------------

    private void Attack(object sender, EventArgs e) {
        if (!canAttack || currentWeapon == null) return;

        Vector2 dir = Player.Instance.playerMovement.GetLookingDirection();
        float baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        float currentAngle = baseAngle + weaponRotationOffsetZ;

        startAngle = currentAngle;
        // salvo la direzione di QUESTO attacco
        currentSwingRight = swingRight;
        // preparo il prossimo attacco
        swingRight = !swingRight;

        isAttacking = true;
        attackElapsed = 0f;

        currentWeaponInstance.GetComponent<IWeapon>().Attack(dir);

        Quaternion meleeEffectRotation = Quaternion.Euler(0, 0, baseAngle);
        GameObject effect = Instantiate(meleeAttackEffect, transform.position + (Vector3)attackCentre, meleeEffectRotation);
        if(effect.TryGetComponent<MeleeEffect>(out MeleeEffect meleeEffect)) {
            meleeEffect.SetDirection(dir);
        }

        attackTimer = Player.Instance.playerStats.playerCurrentStats.GetAttackRate();
        canAttack = false;
    }
    private void HandleWeaponRotation() {
        if (currentWeaponInstance == null) return;

        Vector2 lookingDirection = Player.Instance.playerMovement.GetLookingDirection();
        float baseAngle = Mathf.Atan2(lookingDirection.y, lookingDirection.x) * Mathf.Rad2Deg;

        attackCentre = lookingDirection * distanceFromPlayer; // centro di attacco

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

        HandleSorting(finalAngle);
    }

    private void HandleSorting(float angle) {
        if (currentWeapon.GetComponent<Weapon>().GetWeaponType == Weapon.WeaponType.Melee) {
            if (angle > -90f && angle < 90f)
                currentWeaponInstance.GetComponent<SpriteRenderer>().sortingOrder = 2;
            else
                currentWeaponInstance.GetComponent<SpriteRenderer>().sortingOrder = 0;
        }
    }

    // ----------- GESTIONE ARMI ------------
    // al seguito del pick up si assegna nuova arma
    public void SetCurrentWeapon(GameObject newWeapon) {
        // droppo arma attuale (da fare)
        Destroy(currentWeaponInstance);

        currentWeapon = newWeapon; // assegno nuova arma

        // creo nuova istanza
        currentWeaponInstance = Instantiate(currentWeapon, transform.position, currentWeapon.transform.rotation);
        currentWeaponInstance.transform.SetParent(weaponHolder);
        currentWeaponInstance.transform.localPosition = Vector2.zero;
    }
}
