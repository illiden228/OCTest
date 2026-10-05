using System.Collections.Generic;
using OwlcatTest.Player;
using UnityEngine;
using UnityEngine.Events;

namespace OwlcatTest.Interaction
{
    public enum ActivationMode { Press, EnterVolume }

    [DisallowMultipleComponent]
    public sealed class InteractionTarget : MonoBehaviour
    {
        private static readonly HashSet<InteractionTarget> PressTargets = new();

        [SerializeField] private ActivationMode activationMode;
        [SerializeField] private string prompt = "Взаимодействовать";
        [SerializeField] private Transform anchor;
        [SerializeField, Min(0.1f)] private float range = 2f;
        [SerializeField] private bool singleUse;
        [SerializeField] private UnityEvent onActivated;

        private bool used;

        public static IReadOnlyCollection<InteractionTarget> AvailablePressTargets => PressTargets;
        public string Prompt => prompt;
        public Vector3 Position => anchor != null ? anchor.position : transform.position;
        public Vector3 MarkerPosition
        {
            get
            {
                var targetRenderer = GetComponent<Renderer>();
                return targetRenderer != null
                    ? new Vector3(targetRenderer.bounds.center.x, targetRenderer.bounds.max.y + 0.35f,
                        targetRenderer.bounds.center.z)
                    : Position + Vector3.up * 1.5f;
            }
        }
        public float Range => range;
        public bool CanActivate => isActiveAndEnabled && (!singleUse || !used);
        public UnityEvent OnActivated => onActivated;

        public void SetPrompt(string value) => prompt = value;

        private void OnEnable()
        {
            if (activationMode == ActivationMode.Press) PressTargets.Add(this);
        }

        private void OnDisable() => PressTargets.Remove(this);

        private void OnTriggerEnter(Collider other)
        {
            if (activationMode != ActivationMode.EnterVolume) return;
            var player = other.GetComponentInParent<PlayerMotor>();
            if (player != null) Activate(player);
        }

        public bool Activate(PlayerMotor player)
        {
            if (player == null || !player.IsFree || !CanActivate) return false;
            used = true; // Set before invoking callbacks to prevent re-entrant activation.
            onActivated?.Invoke();
            return true;
        }

        private void OnValidate()
        {
            range = Mathf.Max(0.1f, range);
            if (activationMode == ActivationMode.EnterVolume &&
                (!TryGetComponent<Collider>(out var volume) || !volume.isTrigger))
                Debug.LogWarning("Для взаимодействия через вольюм нужен Collider с Is Trigger на том же объекте.", this);
        }

        private void OnDrawGizmosSelected()
        {
            if (activationMode != ActivationMode.Press) return;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(Position, range);
        }
    }
}
