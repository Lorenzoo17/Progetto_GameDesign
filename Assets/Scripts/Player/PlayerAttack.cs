using System;
using UnityEngine;
using FirstGearGames.SmoothCameraShaker;

public class PlayerAttack : MonoBehaviour {

    [SerializeField] private Transform weaponHolder;
    [SerializeField] private float deadZoneRadius;

    [Header("Centro di attacco (offset rispetto alla direzione di attacco in melee)")]
    public float attackCentreOffset = 1f; // richiamato in WeaponMelee

    private bool canAttack = true;
    private float attackTimer = 0f;

    public GameObject currentWeapon;

    [SerializeField] private Transform attackDirectionUI; // Componente per indicare direzione in cui il player
    // sta guardando
    [SerializeField] private float attackDirectionUIDistanceFromPlayer = 0.5f;
    private Vector2 attackDirection;
    [SerializeField] private bool knockBackWhileAttacking;
    [SerializeField] private float knockBackForce;

    public Transform GetWeaponHolder() {
        return weaponHolder;
    }

    private void Start() {
        InputManager.Instance.OnAttackEvent += Attack;
    }

    private void Update() {
        CalculateAttackDirection();

        if (currentWeapon != null) {
            if (currentWeapon.TryGetComponent<IWeapon>(out IWeapon weapon)) {
                weapon.HandleRotation(weaponHolder, attackDirection);
            }
        }

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

        currentWeapon.GetComponent<IWeapon>().Attack(attackDirection);

        // camera shake e knockback dopo attacco solo se l'arma e' melee
        if (currentWeapon.GetComponent<Weapon>() is WeaponMelee) {

            CameraShakerHandler.Shake(Player.Instance.cameraShakeData);

            if (knockBackWhileAttacking) {
                Player.Instance.playerMovement.ApplyKnockback(-attackDirection, knockBackForce);
            }
        }

        attackTimer = Player.Instance.playerStats.playerCurrentStats.GetAttackRate();
        canAttack = false;
    }

    private void HandleSorting(float angle) {
        if (angle > -90f && angle < 90f)
            currentWeapon.GetComponent<SpriteRenderer>().sortingOrder = 2;
        else
            currentWeapon.GetComponent<SpriteRenderer>().sortingOrder = 0;
    }

    // Attack direction centrata su weaponHolder e non su transform.position in modo da non avere incoerenze
    // nella direzione dell'arma ranged
    private void CalculateAttackDirection() {
        Vector2 direction = InputManager.Instance.CalculateAimDirection(weaponHolder.position);

        if(direction.magnitude > deadZoneRadius) {
            attackDirection = direction.normalized;
        }

        if (attackDirectionUI == null) return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rot = Quaternion.Euler(0, 0, angle);
        attackDirectionUI.rotation = rot;
        // centrato in weaponHolder
        attackDirectionUI.position = weaponHolder.position + (Vector3)(direction.normalized * attackDirectionUIDistanceFromPlayer);
    }

    // ----------- GESTIONE ARMI ------------
    // al seguito del pick up si assegna nuova arma
    public void SetCurrentWeapon(GameObject newWeapon) {
        // droppo arma corrente se presente
        if(currentWeapon != null) {
            currentWeapon.transform.SetParent(null);
            if (currentWeapon.TryGetComponent<Weapon>(out Weapon w)) {
                w.DropWeapon();
            }
            // droppo in basso (momentaneo)
            currentWeapon.transform.position = transform.position + (Vector3.down * 2f);
        }

        // assegno nuova arma
        currentWeapon = newWeapon;
        currentWeapon.transform.SetParent(weaponHolder);
        currentWeapon.transform.localPosition = Vector2.zero;
        currentWeapon.transform.localRotation = Quaternion.Euler(0, 0, 0);
    }
}
