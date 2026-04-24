using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour {

    private Rigidbody2D rb;
    private Vector2 movement;
    private Vector2 lastMoveDirection;
    [SerializeField] private Vector2 lastLookingDirection;
    [SerializeField] private float deadZoneRadius = 0.3f; // dead zone per cambio di direzione con puntamento mouse

    private Animator anim;
    private SpriteRenderer sr;

    [SerializeField] private Transform feetTransform;
    [SerializeField] private float walkVisualEffectSpawnRate = 0.2f;
    private float currentWalkVisualEffectTime;

    // usato per playerAttack o quando viene colpito
    private bool isKnockedBack;
    private float knockbackTimer;
    [SerializeField] private float knockbackDuration = 0.15f;

    // dash
    [SerializeField] private float dodgeDuration = 0.15f;
    [SerializeField] private float dodgeDistance = 5f;
    private bool isDodging;
    private float dodgeTimer;
    private float dodgeCooldownTimer;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    private void Start() {
        if (InputManager.Instance == null) return;

        InputManager.Instance.OnDodgeEvent += OnDodgeEvent_Performed;
    }
    public Vector2 GetDirection() {
        return lastMoveDirection;
    }

    public Vector2 GetLookingDirection() {
        return lastLookingDirection;
    }

    // METHODS

    private void Update() {
        CalculateLookingDirection(); // in base a spostamento del mouse
        PlayerAnimation();
        VisualEffectSpawning();

        if (dodgeCooldownTimer > 0f) {
            dodgeCooldownTimer -= Time.deltaTime;
        }
    }

    private void FixedUpdate() {
        if (rb == null) return;

        if (isDodging) {
            dodgeTimer -= Time.fixedDeltaTime;

            if (dodgeTimer <= 0f) {
                isDodging = false;
            }

            return;
        }

        if (isKnockedBack) {
            knockbackTimer -= Time.fixedDeltaTime;

            if (knockbackTimer <= 0f) {
                isKnockedBack = false;
            }

            return; // blocco il movimento se c'e' knockback
        }

        if (InputManager.Instance == null) return;

        movement = InputManager.Instance.PlayerMovement;

        if (movement != Vector2.zero) {
            lastMoveDirection = movement.normalized;
        }

        // prendo moveSpeed dalle statistiche del player
        float moveSpeed = Player.Instance.playerStats.playerCurrentStats.GetMoveSpeed();
        rb.linearVelocity = movement.normalized * moveSpeed;
    }

    private void OnDodgeEvent_Performed(object sender, EventArgs e) {
        // cooldown
        if (dodgeCooldownTimer > 0f || isDodging) return;

        Vector2 dashDir = lastMoveDirection; // direzione di base di dodge corrisponde a quella di movimento

        // se il player e' fermo, la direzione di dodge corrisponde a quella in cui sta guardando (coincide attack direction)
        if (movement == Vector2.zero) {
            dashDir = lastLookingDirection;
        }

        dashDir = dashDir.normalized;

        // forza = distanza / tempo
        float dashSpeed = dodgeDistance / dodgeDuration;

        rb.linearVelocity = dashDir * dashSpeed;

        isDodging = true;
        dodgeTimer = dodgeDuration;

        anim.SetTrigger("Dodge");

        // cooldown dalle stats
        dodgeCooldownTimer = Player.Instance.playerStats.playerCurrentStats.GetDodgeCooldown();
    }


    private void CalculateLookingDirection() {
        Vector2 mousePos = InputManager.Instance.MousePosition;
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

        Vector2 direction = worldPos - (Vector2)transform.position;

        if (direction.magnitude > deadZoneRadius) {
            lastLookingDirection = direction.normalized;
        }
    }

    private void PlayerAnimation() {
        // flip
        sr.flipX = lastLookingDirection.x < 0;

        anim.SetBool("Moving", movement != Vector2.zero);
    }

    private void VisualEffectSpawning() {
        if(movement != Vector2.zero) {
            if (currentWalkVisualEffectTime <= 0) {

                EffectManager.Instance.SpawnVisualEffect(VisualEffectType.Walk, feetTransform.position, Quaternion.identity);

                currentWalkVisualEffectTime = walkVisualEffectSpawnRate;
            }
            else {
                currentWalkVisualEffectTime -= Time.deltaTime;
            }
        }
        else {
            currentWalkVisualEffectTime = 0;
        }

    }

    // per applicare knockback
    public void ApplyKnockback(Vector2 direction, float force) {
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direction.normalized * force, ForceMode2D.Impulse);

        isKnockedBack = true;
        knockbackTimer = knockbackDuration;
    }
}
