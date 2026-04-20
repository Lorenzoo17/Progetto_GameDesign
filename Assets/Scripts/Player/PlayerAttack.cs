using System;
using UnityEngine;

public class PlayerAttack : MonoBehaviour {

    [SerializeField] private Transform weaponHolder;
    [SerializeField] private Transform handPositionTransform;
    [SerializeField] private float distanceFromPlayer = 0.4f;
    [SerializeField] private float deadZoneRadius = 0.5f;

    [SerializeField] private float playerRotationZOffset;

    private Vector2 lastDirection = Vector2.right;

    private bool canAttack = true;
    private float attackTimer = 0f;

    public GameObject currentWeapon;
    private GameObject currentWeaponInstance;

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

        currentWeaponInstance.GetComponent<IWeapon>().Attack(lastDirection);

        attackTimer = Player.Instance.playerStats.playerCurrentStats.GetAttackRate();
        canAttack = false;
    }

    private void HandleWeaponRotation() {
        Vector2 mousePos = InputManager.Instance.MousePosition;
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

        Vector2 direction = worldPos - (Vector2)handPositionTransform.position;

        if (direction.magnitude > deadZoneRadius) {
            lastDirection = direction.normalized;
        }

        float angle = Mathf.Atan2(lastDirection.y, lastDirection.x) * Mathf.Rad2Deg;
        weaponHolder.rotation = Quaternion.Euler(0, 0, angle);
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
