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

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
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
    }

    private void FixedUpdate() {
        if (rb == null) return;

        if (InputManager.Instance == null) {
            Debug.Log("Input manager non presente");
            return;
        }

        movement = InputManager.Instance.PlayerMovement;
        if (movement != Vector2.zero) {
            lastMoveDirection = movement.normalized;
        }
        // prendo moveSpeed dalle statistiche del player
        float moveSpeed = Player.Instance.playerStats.playerCurrentStats.GetMoveSpeed();
        rb.linearVelocity = movement.normalized * moveSpeed;
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
}
