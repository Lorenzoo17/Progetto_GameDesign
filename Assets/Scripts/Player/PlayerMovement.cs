using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour {

    [SerializeField] private float speed;
    [SerializeField] private float currentSpeed;

    [SerializeField] private Vector2 movement;

    private Rigidbody2D rb;

    // GET AND SET
    public float GetSpeed => speed;
    public float GetCurrentSpeed => currentSpeed;
    public Vector2 GetMovement => movement;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();

        SetSpeed(speed); // imposto currentspeed = speed
    }

    // METHODS
    private void Update() {
        // Da sostituire con Input system
        movement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }
    private void FixedUpdate() {
        if (rb == null) return;

        rb.linearVelocity = movement.normalized * currentSpeed;
    }

    private void SetSpeed(float value) {
        currentSpeed = value;
    }
}
