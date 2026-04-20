using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour {

    private Rigidbody2D rb;
    private Vector2 movement;

    private Animator anim;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
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
        // prendo moveSpeed dalle statistiche del player
        float moveSpeed = Player.Instance.playerStats.playerCurrentStats.GetMoveSpeed();
        rb.linearVelocity = movement.normalized * moveSpeed;
    }

    private void PlayerAnimation() {
        Vector2 mousePos = InputManager.Instance.MousePosition;
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

        Vector2 mouseDirection = (worldPos - (Vector2)transform.position).normalized;

        anim.SetFloat("MouseX", mouseDirection.x);
        anim.SetFloat("MouseY", mouseDirection.y);

        anim.SetBool("Moving", movement != Vector2.zero);
    }
}
