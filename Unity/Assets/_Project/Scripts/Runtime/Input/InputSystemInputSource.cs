using EchoShift.Replay;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoShift.Input
{
    public enum InputPromptDevice : byte
    {
        Keyboard,
        Gamepad
    }

    public sealed class InputSystemInputSource : MonoBehaviour, IInputSource
    {
        private const string ActionMapName = "Gameplay";
        private const string MoveActionName = "Move";
        private const string InteractActionName = "Interact";
        private const string EndLoopActionName = "EndLoop";

        [SerializeField] private InputActionAsset inputActions;

        private InputActionMap _gameplayMap;
        private InputAction _moveAction;
        private InputAction _interactAction;
        private InputAction _endLoopAction;
        private bool _interactLatched;
        private bool _endLoopLatched;
        private bool _callbacksAttached;

        public bool HasValidReferences =>
            inputActions != null &&
            _gameplayMap != null &&
            _moveAction != null &&
            _interactAction != null &&
            _endLoopAction != null;
        public InputPromptDevice LastPromptDevice { get; private set; } =
            InputPromptDevice.Keyboard;

        public void Configure(InputActionAsset actions)
        {
            bool wasEnabled = isActiveAndEnabled && Application.isPlaying;
            if (wasEnabled)
            {
                DetachCallbacks();
            }

            inputActions = actions;
            ResolveActions();

            if (wasEnabled)
            {
                AttachCallbacks();
                _gameplayMap?.Enable();
            }
        }

        private void Awake()
        {
            ResolveActions();
        }

        private void OnEnable()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            ResolveActions();
            AttachCallbacks();
            _gameplayMap?.Enable();
        }

        private void OnDisable()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            DetachCallbacks();
            _gameplayMap?.Disable();
            _interactLatched = false;
            _endLoopLatched = false;
        }

        public InputCommand Sample(int tick)
        {
            Vector2 move = _moveAction != null
                ? _moveAction.ReadValue<Vector2>()
                : Vector2.zero;
            if (move.sqrMagnitude > 1f)
            {
                move.Normalize();
            }

            InputButtonFlags buttons = InputButtonFlags.None;
            if (_interactLatched)
            {
                buttons |= InputButtonFlags.Interact;
                _interactLatched = false;
            }

            if (_endLoopLatched)
            {
                buttons |= InputButtonFlags.EndLoop;
                _endLoopLatched = false;
            }

            return new InputCommand(tick, move, buttons);
        }

        private void ResolveActions()
        {
            _gameplayMap = inputActions?.FindActionMap(ActionMapName, false);
            _moveAction = _gameplayMap?.FindAction(MoveActionName, false);
            _interactAction = _gameplayMap?.FindAction(InteractActionName, false);
            _endLoopAction = _gameplayMap?.FindAction(EndLoopActionName, false);
        }

        private void AttachCallbacks()
        {
            if (_callbacksAttached || _interactAction == null || _endLoopAction == null)
            {
                return;
            }

            _interactAction.performed += OnInteractPerformed;
            _endLoopAction.performed += OnEndLoopPerformed;
            _moveAction.performed += OnMovePerformed;
            _callbacksAttached = true;
        }

        private void DetachCallbacks()
        {
            if (!_callbacksAttached)
            {
                return;
            }

            _interactAction.performed -= OnInteractPerformed;
            _endLoopAction.performed -= OnEndLoopPerformed;
            _moveAction.performed -= OnMovePerformed;
            _callbacksAttached = false;
        }

        private void OnInteractPerformed(InputAction.CallbackContext context)
        {
            UpdatePromptDevice(context);
            _interactLatched = true;
        }

        private void OnEndLoopPerformed(InputAction.CallbackContext context)
        {
            UpdatePromptDevice(context);
            _endLoopLatched = true;
        }

        private void OnMovePerformed(InputAction.CallbackContext context)
        {
            UpdatePromptDevice(context);
        }

        public void SetPromptDeviceForTests(InputPromptDevice device)
        {
            LastPromptDevice = device;
        }

        private void UpdatePromptDevice(InputAction.CallbackContext context)
        {
            LastPromptDevice = context.control?.device is Gamepad
                ? InputPromptDevice.Gamepad
                : InputPromptDevice.Keyboard;
        }
    }
}
