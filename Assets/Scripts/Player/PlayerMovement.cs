using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour {

    private Rigidbody2D rb;
    private Vector2 movement;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
    }

    // METHODS

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
}
