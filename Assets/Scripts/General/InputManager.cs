using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private InputSystem_Actions playerInputActions;

    public Vector2 PlayerMovement { get; private set; }
    public Vector2 MousePosition { get; private set; }
    public Vector2 AimDirection { get; private set; }

    private InputAction attackAction;

    public bool inputEnabled = true;

    public event EventHandler OnAttackEvent;
    public event EventHandler OnDodgeEvent;
    public event EventHandler OnInteractEvent;
    public event Action<int> OnMutagenPressed;

    private Camera cam;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        playerInputActions = new InputSystem_Actions();
        playerInputActions.Player.Enable();

        // Movement
        playerInputActions.Player.Move.performed += Move_performed;
        playerInputActions.Player.Move.canceled += Move_canceled;

        // Attack
        attackAction = playerInputActions.Player.Attack;

        // Dash
        playerInputActions.Player.Dash.performed += Dash_performed;

        // Interact
        playerInputActions.Player.Interact.performed += Interact_performed;

        // Mutagens
        playerInputActions.Player.Mutagen1.performed += Mutagen1;
        playerInputActions.Player.Mutagen2.performed += Mutagen2;

        // Pause
        playerInputActions.Player.Pause.performed += ctx =>
        {
            if (Player.Instance == null) return;
            if (Player.Instance.isDead) return;

            PauseMenuManager pauseMenu = FindObjectOfType<PauseMenuManager>();
            if (pauseMenu != null)
            {
                if (pauseMenu.IsPaused())
                {
                    pauseMenu.Resume();
                }
                else
                {
                    pauseMenu.Pause();
                }
            }
        };
    }

    private void Start()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        if (!inputEnabled) return;
        if (Player.Instance == null) return;
        if (Player.Instance.isDead) return;

        if (attackAction.IsPressed())
        {
            OnAttackEvent?.Invoke(this, EventArgs.Empty);
        }

        AimDirection = CalculateAimDirection(Player.Instance.transform.position);
    }

    public Vector2 CalculateAimDirection(Vector2 reference)
    {
        if (cam == null)
        {
            cam = Camera.main;
            if (cam == null) return Vector2.zero;
        }

        Vector2 mousePos = playerInputActions.Player.Aim.ReadValue<Vector2>();
        Vector2 stick = playerInputActions.Player.AimStick.ReadValue<Vector2>();

        Vector2 finalDirection = Vector2.zero;

        // controller
        if (stick.magnitude > 0.2f)
        {
            finalDirection = stick.normalized;
        }
        // mouse
        else
        {
            if (cam == null)
                return Vector2.zero;

            Vector2 worldPos = cam.ScreenToWorldPoint(mousePos);
            Vector2 direction = worldPos - reference;

            if (direction.magnitude > 0.2f)
            {
                finalDirection = direction.normalized;
            }
        }

        return finalDirection;
    }

    private void Move_performed(InputAction.CallbackContext ctx)
    {
        PlayerMovement = ctx.ReadValue<Vector2>();
    }

    private void Move_canceled(InputAction.CallbackContext ctx)
    {
        PlayerMovement = Vector2.zero;
    }

    private void Dash_performed(InputAction.CallbackContext obj)
    {
        OnDodgeEvent?.Invoke(this, EventArgs.Empty);
    }

    private void Interact_performed(InputAction.CallbackContext obj)
    {
        OnInteractEvent?.Invoke(this, EventArgs.Empty);
    }

    private void Mutagen1(InputAction.CallbackContext ctx)
    {
        OnMutagenPressed?.Invoke(0);
    }

    private void Mutagen2(InputAction.CallbackContext ctx)
    {
        OnMutagenPressed?.Invoke(1);
    }

    private void OnDestroy()
    {
        if (playerInputActions == null) return;

        playerInputActions.Player.Move.performed -= Move_performed;
        playerInputActions.Player.Move.canceled -= Move_canceled;

        playerInputActions.Player.Dash.performed -= Dash_performed;
        playerInputActions.Player.Interact.performed -= Interact_performed;

        playerInputActions.Player.Mutagen1.performed -= Mutagen1;
        playerInputActions.Player.Mutagen2.performed -= Mutagen2;

        playerInputActions.Player.Disable();
    }
}