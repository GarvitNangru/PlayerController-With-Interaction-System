using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace PlayerController
{
    public class InputHandler : Singleton<InputHandler>
    {
        #region Input Actions
        private PlayerInput m_input = null;
        private InputAction m_move;
        private InputAction m_look;
        private InputAction m_sprint;
        private InputAction m_jump;
        private InputAction m_crouch;
        private InputAction m_useAction;
        private InputAction m_inventory;

        #endregion

        #region Static Actions/Events

        public event Action UseActionPerformedEvent;
        public event Action OnInventoryInputEvent;
        public event Action<bool> OnInteractInputEvent;

        #endregion

        #region Data
        [Space(10), Header("Input Data")]
        [SerializeField] private CameraInputData cameraInputData = null;
        [SerializeField] private MovementInputData movementInputData = null;
        #endregion


        private bool IsCamPaused = false;

        #region BuiltIn Methods
        protected override void Awake()
        {
            m_input = GetComponent<PlayerInput>();
            SetInputsActions();
        }
        void Start()
        {
            cameraInputData.ResetInput();
            movementInputData.ResetInput();
        }
        private void OnEnable()
        {
            m_sprint.performed += Sprint;
            m_sprint.canceled += Sprint;

            m_jump.performed += Jump;
            //   m_jump.canceled += Jump;

            m_crouch.performed += Crouch;
            m_crouch.canceled += Crouch;

            m_inventory.performed += InventoryInput;

            m_useAction.performed += UseActionPerformed;


            GameEvents.OnUIToggleEvent += OnUIToggleEvent;
        }

        private void OnDisable()
        {
            m_sprint.performed -= Sprint;
            m_sprint.canceled -= Sprint;

            m_jump.performed -= Jump;
            //    m_jump.canceled -= Jump;

            m_crouch.performed -= Crouch;
            m_crouch.canceled -= Crouch;

            m_inventory.performed -= InventoryInput;

            m_useAction.performed -= UseActionPerformed;


            GameEvents.OnUIToggleEvent -= OnUIToggleEvent;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Comma))
                ToggleCamera();

            GetCameraInput();
            GetMovementInputData();
        }
        #endregion

        #region input Methods
        private void SetInputsActions()
        {
            m_move = m_input.actions["Movement"];
            m_look = m_input.actions["Look"];
            m_sprint = m_input.actions["Sprint"];
            m_jump = m_input.actions["Jump"];
            m_crouch = m_input.actions["Crouch"];
            m_useAction = m_input.actions["UseEquippedItem"];
            m_inventory = m_input.actions["Inventory"];
            //        m_interact = m_input.actions["Interact"];
        }

        private void Sprint(InputAction.CallbackContext ctx)
        {
            movementInputData.IsRunning = ctx.performed;
        }
        private void Jump(InputAction.CallbackContext ctx)
        {
            movementInputData.JumpClicked = ctx.performed;
        }
        private void Crouch(InputAction.CallbackContext ctx)
        {
            movementInputData.CrouchClicked = ctx.performed;
        }

        #endregion
        #region Custom Methods

        void ToggleCamera()
        {
            if (m_look.enabled)
                m_look.Disable();
            else
                m_look.Enable();
        }

        public void TogglePlayerCamInput(bool value)
        {
            IsCamPaused = !value;
        }

        #region GetInput Methods
        void GetCameraInput()
        {
            if (IsCamPaused)
            {
                cameraInputData.ResetInput();
                return;
            }

            Vector2 mouse = GetMouse();
            cameraInputData.InputVectorX = mouse.x;
            cameraInputData.InputVectorY = mouse.y;

            //cameraInputData.ZoomClicked = Input.GetMouseButtonDown(1);
            //cameraInputData.ZoomReleased = Input.GetMouseButtonUp(1);
        }

        void GetMovementInputData()
        {
            Vector2 move = GetMovement();
            movementInputData.InputVectorX = move.x;
            movementInputData.InputVectorY = move.y;
        }

        private Vector2 GetMovement()
        {
            return m_move.ReadValue<Vector2>();
        }

        public Vector2 GetMouse()
        {
            return m_look.ReadValue<Vector2>();
        }

        #endregion

        #region Methods

        private void OnUIToggleEvent(bool obj)
        {
            if (obj)
            {
                EnableOnlyUIControls();

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                EnableAllControls();

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void EnableOnlyUIControls()
        {
            m_sprint.Disable();
            m_move.Disable();
            m_crouch.Disable();
            m_look.Disable();
        }

        private void EnableAllControls()
        {
            ToggleInputActions(true);
        }

        private void InventoryInput(InputAction.CallbackContext obj)
        {
            OnInventoryInputEvent?.Invoke();
        }
        private void InteractPerformed(InputAction.CallbackContext obj)
        {
            OnInteractInputEvent?.Invoke(obj.performed);
        }

        private void UseActionPerformed(InputAction.CallbackContext obj)
        {
            UseActionPerformedEvent?.Invoke();
        }

        #endregion

        #region Custom Hotkey Methods

        public async void MapAction(string ActionName, InputActionType type = InputActionType.Button, Action<InputAction> callback = null)
        {
            InputAction ActionToMap;
            bool CheckIfActionAlreadyExists()
            {
                ActionToMap = m_input.actions.FindAction(ActionName, false);
                if (ActionToMap != null)
                    return true;
                else
                    return false;
            }

            m_input.currentActionMap.Disable();
            ToggleInputActions(false);

            Debug.Log("STARTT ENTERING INPUT");
            string controlPath = await ListenToInput();
            if (controlPath == string.Empty)
                return;
            Debug.Log("Got INPUT     --------- " + controlPath);

            if (IsBindingInUse(controlPath))
            {
                Debug.Log("-------------------------------------------------------------");
                Debug.LogError("Binding already in use CANNOT PROCEED    " + controlPath);

                return;
            }


            if (CheckIfActionAlreadyExists())
            {
                //       ActionToMap.RemoveBindingOverride(0);
                ActionToMap.ApplyBindingOverride(0, controlPath);
                //       ActionToMap.AddBinding(controlPath);

                callback?.Invoke(ActionToMap);
            }
            else
            {
                m_input.currentActionMap.AddAction(ActionName, type, controlPath);
                ActionToMap = m_input.actions[ActionName];

                callback?.Invoke(ActionToMap);
            }

            m_input.currentActionMap.Enable();
            ToggleInputActions(true);

        }


        string NewBindPath = string.Empty;
        private async Task<string> ListenToInput()
        {
            NewBindPath = string.Empty;

            InputSystem.onEvent += BindingEventListener;

            int seconds = 0;
            while (NewBindPath == string.Empty && seconds <= 20)
            {
                await Task.Delay(500);
                seconds++;
            }

            Debug.Log($"Finished Waiting Readed path : {NewBindPath}");

            return NewBindPath;
        }

        private void BindingEventListener(InputEventPtr ptr, InputDevice device)
        {
            var control = ptr.GetFirstButtonPressOrNull();

            if (control != null)
            {
                Debug.Log($"Key Pressed  :   {control.path}");

                if (IsHotKeyEligible(control))
                {
                    Debug.Log($"TYPE  :  {control.GetType()}");

                    NewBindPath = control.path;
                    InputSystem.onEvent -= BindingEventListener;
                }
            }
        }

        private bool IsHotKeyEligible(InputControl control)
        {
            switch (control.name)
            {
                case "1":
                    return true;
                case "2":
                    return true;
                case "3":
                    return true;
                case "4":
                    return true;
                case "5":
                    return true;
                case "6":
                    return true;
                case "7":
                    return true;

                default:
                    return false;
            }
        }

        private bool IsBindingInUse(string newBindingPath)
        {
            //  var newBinding = actionToRebind.bindings[0];

            var bindings = m_input.currentActionMap.bindings;
            foreach (var binding in bindings)
            {
                //if (binding.action == newBinding.action)
                //    continue;
                if (binding.path == newBindingPath)
                    return true;
            }

            return false;
        }
        private void ToggleInputActions(bool toggle)
        {
            foreach (var action in m_input.currentActionMap.actions)
            {
                if (toggle)
                    action.Enable();
                else
                    action.Disable();
            }
        }
        #endregion
        #endregion
    }
}
