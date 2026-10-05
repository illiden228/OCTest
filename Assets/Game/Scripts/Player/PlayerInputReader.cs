using UnityEngine;
using UnityEngine.InputSystem;

namespace OwlcatTest.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerInputReader : MonoBehaviour
    {
        [SerializeField] private InputActionAsset actions;

        private InputActionAsset runtimeActions;
        private InputActionMap playerMap;
        private InputAction move;
        private InputAction sprint;
        private InputAction interact;

        public Vector2 Move => move?.ReadValue<Vector2>() ?? Vector2.zero;
        public bool SprintHeld => sprint?.IsPressed() ?? false;
        public bool InteractPressed => interact?.WasPressedThisFrame() ?? false;
        public string InteractBindingLabel
        {
            get
            {
                if (interact == null || interact.bindings.Count == 0) return "E";
                var path = interact.bindings[0].effectivePath;
                var slash = path?.LastIndexOf('/') ?? -1;
                if (slash < 0 || slash == path.Length - 1) return "E";
                var key = path[(slash + 1)..];
                return key.Length == 1 ? key.ToUpperInvariant() : key switch
                {
                    "space" => "Space",
                    "leftShift" or "rightShift" => "Shift",
                    "enter" or "numpadEnter" => "Enter",
                    _ => key
                };
            }
        }

        private void Awake()
        {
            if (actions == null)
            {
                Debug.LogError("Player input actions are not assigned.", this);
                enabled = false;
                return;
            }

            runtimeActions = Instantiate(actions);
            playerMap = runtimeActions.FindActionMap("Player", true);
            move = playerMap.FindAction("Move", true);
            sprint = playerMap.FindAction("Sprint", true);
            interact = playerMap.FindAction("Interact", true);
        }

        private void OnEnable() => playerMap?.Enable();
        private void OnDisable() => playerMap?.Disable();

        private void OnDestroy()
        {
            if (runtimeActions != null) Destroy(runtimeActions);
        }
    }
}
