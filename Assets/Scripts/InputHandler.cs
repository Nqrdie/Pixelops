using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class InputHandler : MonoBehaviour
{

    [Header("InputSystem")]
    [SerializeField] private InputActionAsset playerControls;

    [Header("Action map name reference")]
    [SerializeField] private string actionMapName = "Player";

    [Header("Action name reference")]
    [SerializeField] private string movementActionName = "Movement";
    [SerializeField] private string jumpActionName = "Jump";
    [SerializeField] private string sprintActionName = "Sprint";
    [SerializeField] private string shootActionName = "Shoot";
    [SerializeField] private string aimActionName = "Aim";
    [SerializeField] private string reloadActionName = "Reload";
    [SerializeField] private string switchWeaponActionName = "Swap";

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private InputAction shootAction;
    private InputAction aimAction;
    private InputAction reloadAction;
    public InputAction switchWeaponAction;

    public int switchWeaponIndex { get; private set; }

    public Vector2 moveInput { get; private set; }

    public float sprintValue { get; private set; }

    public bool jumpTriggered { get; private set; }

    public bool shootTriggered { get; set; }

    public bool aimTriggered { get; private set; }

    public bool reloadTriggered { get; private set; }

    public int switchWeaponValue { get; private set; }


    public static InputHandler Instance { get; private set; }

    private void Awake()
    {

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        moveAction = playerControls.FindActionMap(actionMapName).FindAction(movementActionName);
        sprintAction = playerControls.FindActionMap(actionMapName).FindAction(sprintActionName);
        jumpAction = playerControls.FindActionMap(actionMapName).FindAction(jumpActionName);
        shootAction = playerControls.FindActionMap(actionMapName).FindAction(shootActionName);
        aimAction = playerControls.FindActionMap(actionMapName).FindAction(aimActionName);
        reloadAction = playerControls.FindActionMap(actionMapName).FindAction(reloadActionName);
        switchWeaponAction = playerControls.FindActionMap(actionMapName).FindAction(switchWeaponActionName);
        RegisterInputActions();
    }

    private void RegisterInputActions()
    {
        moveAction.performed += context => moveInput = context.ReadValue<Vector2>();
        moveAction.canceled += context => moveInput = Vector2.zero;

        sprintAction.performed += context => sprintValue = context.ReadValue<float>();
        sprintAction.canceled += context => sprintValue = 0f;

        jumpAction.performed += context => jumpTriggered = true;
        jumpAction.canceled += context => jumpTriggered = false;

        shootAction.performed += context => shootTriggered = true;
        shootAction.canceled += context => shootTriggered = false;

        aimAction.performed += context => aimTriggered = true;
        aimAction.canceled += context => aimTriggered = false;

        reloadAction.performed += context => reloadTriggered = true;
        reloadAction.canceled += context => reloadTriggered = false;

        switchWeaponAction.performed += OnSwitchWeapon;
        switchWeaponAction.canceled -= OnSwitchWeapon;
    }

    private void OnEnable()
    {
        moveAction.Enable();
        sprintAction.Enable();
        jumpAction.Enable();    
        shootAction.Enable();
        aimAction.Enable();
        reloadAction.Enable();
        switchWeaponAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        sprintAction.Disable();
        jumpAction.Disable();
        shootAction.Disable();
        aimAction.Disable();
        reloadAction.Disable();
        switchWeaponAction.Disable();
    }

    private void OnSwitchWeapon(InputAction.CallbackContext context)
    {
        switchWeaponIndex = (int)context.ReadValue<float>();
    }
}
