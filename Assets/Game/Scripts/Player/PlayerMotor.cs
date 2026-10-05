using UnityEngine;

namespace OwlcatTest.Player
{
    public enum PlayerMode { Free, Dialogue, Traversal }

    [RequireComponent(typeof(CharacterController), typeof(PlayerInputReader))]
    [DisallowMultipleComponent]
    public sealed class PlayerMotor : MonoBehaviour
    {
        [SerializeField] private Transform cameraTransform;
        [SerializeField, Min(0f)] private float walkSpeed = 4f;
        [SerializeField, Min(1f)] private float sprintMultiplier = 1.6f;
        [SerializeField, Min(0f)] private float turnSpeedDegrees = 720f;
        [SerializeField] private float gravity = -25f;
        [SerializeField] private float fallRecoveryHeight = -8f;

        private CharacterController controller;
        private PlayerInputReader input;
        private float verticalSpeed;
        private Vector3 startPosition;
        private Quaternion startRotation;

        public PlayerMode Mode { get; private set; } = PlayerMode.Free;
        public bool IsFree => Mode == PlayerMode.Free;
        public bool SprintHeld => input.SprintHeld;
        public int LastModeExitFrame { get; private set; } = -1;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            input = GetComponent<PlayerInputReader>();
            startPosition = transform.position;
            startRotation = transform.rotation;
            if (cameraTransform == null && Camera.main != null)
                cameraTransform = Camera.main.transform;
        }

        private void Update()
        {
            if (!IsFree) return;
            if (transform.position.y < fallRecoveryHeight)
            {
                controller.enabled = false;
                transform.SetPositionAndRotation(startPosition, startRotation);
                controller.enabled = true;
                verticalSpeed = 0f;
                return;
            }

            var axis = Vector2.ClampMagnitude(input.Move, 1f);
            var forward = cameraTransform != null ? cameraTransform.forward : Vector3.forward;
            forward.y = 0f;
            forward.Normalize();
            var right = cameraTransform != null ? cameraTransform.right : Vector3.right;
            right.y = 0f;
            right.Normalize();

            var direction = Vector3.ClampMagnitude(right * axis.x + forward * axis.y, 1f);
            if (direction.sqrMagnitude > 0.001f)
            {
                var facing = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, facing, turnSpeedDegrees * Time.deltaTime);
            }

            if (controller.isGrounded && verticalSpeed < 0f)
                verticalSpeed = -2f;
            verticalSpeed += gravity * Time.deltaTime;

            var speed = walkSpeed * (input.SprintHeld ? sprintMultiplier : 1f);
            controller.Move((direction * speed + Vector3.up * verticalSpeed) * Time.deltaTime);
        }

        public bool TryEnterMode(PlayerMode mode)
        {
            if (mode == PlayerMode.Free || !IsFree) return false;
            Mode = mode;
            verticalSpeed = 0f;
            return true;
        }

        public void ExitMode(PlayerMode mode)
        {
            if (Mode != mode) return;
            Mode = PlayerMode.Free;
            LastModeExitFrame = Time.frameCount;
            verticalSpeed = 0f;
        }

        private void OnValidate()
        {
            walkSpeed = Mathf.Max(0f, walkSpeed);
            sprintMultiplier = Mathf.Max(1f, sprintMultiplier);
            turnSpeedDegrees = Mathf.Max(0f, turnSpeedDegrees);
        }
    }
}
