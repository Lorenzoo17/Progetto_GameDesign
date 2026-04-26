using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance {  get; private set; }

    private InputSystem_Actions playerInputActions;

    public Vector2 PlayerMovement {  get; private set; }
    public Vector2 MousePosition { get; private set; }
    public Vector2 AimDirection { get; private set; }
    public event EventHandler OnAttackEvent; // richiamato in playerattack (con iscrizione all'evento)
    public event EventHandler OnDodgeEvent;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        playerInputActions = new InputSystem_Actions(); // input action definito in editor
        playerInputActions.Player.Enable(); // Abilito input action relativo al player

        // Player movement
        playerInputActions.Player.Move.performed += Move_performed;
        playerInputActions.Player.Move.canceled += Move_canceled;

        // Player attack
        playerInputActions.Player.Attack.performed += Attack_performed;

        // Player dash
        playerInputActions.Player.Dash.performed += Dash_performed;
    }

    private void Dash_performed(InputAction.CallbackContext obj) {
        OnDodgeEvent?.Invoke(this, EventArgs.Empty);
    }

    private void Attack_performed(InputAction.CallbackContext obj) {
        OnAttackEvent?.Invoke(this, EventArgs.Empty);
    }

    private void Update() {
        if (Player.Instance == null) return;

        CalculateAimDirection(Player.Instance.transform.position);
    }

    public Vector2 CalculateAimDirection(Vector2 reference) {
        Vector2 mousePos = playerInputActions.Player.Aim.ReadValue<Vector2>();
        Vector2 stick = playerInputActions.Player.AimStick.ReadValue<Vector2>();

        Vector2 finalDirection = Vector2.zero;

        // controller
        if (stick.magnitude > 0.2f) {
            finalDirection = stick.normalized;
        }
        // mouse
        else {
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            Vector2 direction = worldPos - reference;

            if (direction.magnitude > 0.2f) {
                finalDirection = direction.normalized;
            }
        }

        return finalDirection;
        //Debug.Log(MousePosition);
    }

    private void Move_performed(InputAction.CallbackContext ctx) {
        PlayerMovement = ctx.ReadValue<Vector2>();
    }
    private void Move_canceled(InputAction.CallbackContext obj) {
        PlayerMovement = Vector2.zero;
    }
}
