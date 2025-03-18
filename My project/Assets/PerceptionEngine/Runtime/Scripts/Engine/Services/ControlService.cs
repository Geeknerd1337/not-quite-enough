using UnityEngine;
using UnityEngine.InputSystem;



namespace Perception.Engine
{
    public class ControlService : PerceptionService
    {
        public bool CursorLocked { get; set; }

        public PlayerInputActions PlayerActions;
        public InputAction PlayerMovementAction;
        public InputAction PlayerRunAction;
        public InputAction PlayerJumpAction;
        public InputAction PlayerCrouchAction;
        public InputAction PlayerToggleCrouchAction;
        public InputAction PlayerLookAction;
        public InputAction PlayerInteractAction;
        public InputAction PlayerUseAction;
        public InputAction PlayerPauseAction;


        public override void Awake()
        {
            base.Awake();
            CursorLocked = true;
            PlayerActions = new PlayerInputActions();
            PlayerMovementAction = PlayerActions.Player.Movement;
            PlayerRunAction = PlayerActions.Player.Run;
            PlayerJumpAction = PlayerActions.Player.Jump;
            PlayerCrouchAction = PlayerActions.Player.Crouch;
            PlayerToggleCrouchAction = PlayerActions.Player.ToggleCrouch;
            PlayerLookAction = PlayerActions.Player.Look;
            PlayerInteractAction = PlayerActions.Player.Interact;
            PlayerUseAction = PlayerActions.Player.Use;
            PlayerPauseAction = PlayerActions.Player.Pause;

        }

        public override void Update()
        {
            base.Update();
            if (!CursorLocked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

    }
}
