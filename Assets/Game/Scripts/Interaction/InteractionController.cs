using OwlcatTest.Player;
using UnityEngine;

namespace OwlcatTest.Interaction
{
    [RequireComponent(typeof(PlayerMotor), typeof(PlayerInputReader))]
    [DisallowMultipleComponent]
    public sealed class InteractionController : MonoBehaviour
    {
        [SerializeField] private InteractionPromptView promptView;
        [SerializeField] private Vector3 eyeOffset = new Vector3(0f, 1f, 0f);
        [SerializeField] private LayerMask obstructionMask;

        private PlayerMotor player;
        private PlayerInputReader input;
        private InteractionTarget focused;

        private void Awake()
        {
            player = GetComponent<PlayerMotor>();
            input = GetComponent<PlayerInputReader>();
        }

        private void Update()
        {
            focused = player.IsFree && player.LastModeExitFrame != Time.frameCount ? FindClosest() : null;
            if (promptView != null)
                promptView.Show(focused != null ? focused.Prompt : null,
                    focused != null ? focused.MarkerPosition : Vector3.zero,
                    input.InteractBindingLabel);

            if (focused != null && input.InteractPressed)
                focused.Activate(player);
        }

        private InteractionTarget FindClosest()
        {
            InteractionTarget best = null;
            var bestDistance = float.PositiveInfinity;
            var origin = transform.position;

            foreach (var candidate in InteractionTarget.AvailablePressTargets)
            {
                if (!candidate.CanActivate) continue;
                var distance = (candidate.Position - origin).sqrMagnitude;
                if (distance > candidate.Range * candidate.Range || distance >= bestDistance) continue;
                if (obstructionMask.value != 0 &&
                    Physics.Linecast(origin + eyeOffset, candidate.Position, obstructionMask,
                        QueryTriggerInteraction.Ignore)) continue;
                best = candidate;
                bestDistance = distance;
            }

            return best;
        }

        private void OnDisable() => promptView?.Show(null);
    }
}
