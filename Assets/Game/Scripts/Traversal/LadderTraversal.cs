using OwlcatTest.Player;
using UnityEngine;

namespace OwlcatTest.Traversal
{
    [DisallowMultipleComponent]
    public sealed class LadderTraversal : MonoBehaviour
    {
        [SerializeField] private PlayerMotor player;
        [SerializeField] private Transform bottomPoint;
        [SerializeField] private Transform topPoint;
        [SerializeField] private Transform visual;
        [SerializeField, Min(1f)] private float height = 4f;
        [SerializeField, Min(0.1f)] private float ascentSpeed = 2f;
        [SerializeField, Min(0.1f)] private float descentSpeed = 2.5f;
        [SerializeField, Min(1f)] private float sprintMultiplier = 1.75f;

        private CharacterController controller;
        private Transform destination;
        private Vector3 entry;
        private bool approaching;

        public bool IsTraversing => destination != null;

        public void BeginAscent() => Begin(bottomPoint, topPoint);
        public void BeginDescent() => Begin(topPoint, bottomPoint);

        private void Begin(Transform from, Transform to)
        {
            if (IsTraversing || player == null || from == null || to == null) return;
            if (!player.TryEnterMode(PlayerMode.Traversal)) return;
            controller = player.GetComponent<CharacterController>();
            if (controller != null) controller.enabled = false;
            entry = from.position;
            destination = to;
            approaching = true;
        }

        private void Update()
        {
            if (!IsTraversing) return;
            var ascending = destination == topPoint;
            var speed = (ascending ? ascentSpeed : descentSpeed) *
                        (player.SprintHeld ? sprintMultiplier : 1f);
            var target = approaching ? entry : destination.position;
            player.transform.position = Vector3.MoveTowards(player.transform.position, target, speed * Time.deltaTime);
            if ((player.transform.position - target).sqrMagnitude > 0.0001f) return;
            if (approaching)
            {
                approaching = false;
                return;
            }
            Finish();
        }

        private void Finish()
        {
            destination = null;
            if (controller != null) controller.enabled = true;
            player.ExitMode(PlayerMode.Traversal);
        }

        private void OnDisable()
        {
            if (IsTraversing) Finish();
        }

        private void OnValidate()
        {
            height = Mathf.Max(1f, height);
            ascentSpeed = Mathf.Max(0.1f, ascentSpeed);
            descentSpeed = Mathf.Max(0.1f, descentSpeed);
            sprintMultiplier = Mathf.Max(1f, sprintMultiplier);
            if (bottomPoint != null && topPoint != null)
            {
                var top = topPoint.localPosition;
                top.y = bottomPoint.localPosition.y + height;
                topPoint.localPosition = top;
            }
            if (visual != null)
            {
                visual.localPosition = new Vector3(0f, height * 0.5f, 0f);
                var scale = visual.localScale;
                scale.y = height;
                visual.localScale = scale;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (bottomPoint == null || topPoint == null) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(bottomPoint.position, topPoint.position);
            Gizmos.DrawWireSphere(bottomPoint.position, 0.25f);
            Gizmos.DrawWireSphere(topPoint.position, 0.25f);
        }
    }
}
