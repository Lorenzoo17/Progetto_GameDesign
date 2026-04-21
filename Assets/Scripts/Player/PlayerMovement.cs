using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour {

    private Rigidbody2D rb;
    private Vector2 movement;
    private Vector2 lastMoveDirection;

    private Animator anim;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    public Vector2 GetDirection() {
        return lastMoveDirection;
    }

    // METHODS

    private void Update() {
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

    private void PlayerAnimation() {
        Vector2 mousePos = InputManager.Instance.MousePosition;
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

        Vector2 mouseDirection = (worldPos - (Vector2)transform.position).normalized;

        anim.SetFloat("MouseX", lastMoveDirection.x);
        anim.SetFloat("MouseY", lastMoveDirection.y);

        anim.SetBool("Moving", movement != Vector2.zero);
    }
}
