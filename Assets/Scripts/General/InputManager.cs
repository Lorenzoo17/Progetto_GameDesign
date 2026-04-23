using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance {  get; private set; }

    private InputSystem_Actions playerInputActions;

    public Vector2 PlayerMovement {  get; private set; }
    public Vector2 MousePosition { get; private set; }
    public EventHandler OnAttackEvent; // richiamato in playerattack (con iscrizione all'evento)

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

        // Player attacl
        playerInputActions.Player.Attack.performed += Attack_performed;
    }

    private void Attack_performed(InputAction.CallbackContext obj) {
        OnAttackEvent?.Invoke(this, EventArgs.Empty);
    }

    private void Update() {
        MousePosition = playerInputActions.Player.Aim.ReadValue<Vector2>();
        //Debug.Log(MousePosition);
    }

    private void Move_performed(InputAction.CallbackContext ctx) {
        PlayerMovement = ctx.ReadValue<Vector2>();
    }
    private void Move_canceled(InputAction.CallbackContext obj) {
        PlayerMovement = Vector2.zero;
    }
}
