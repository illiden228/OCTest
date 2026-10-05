using UnityEngine;

namespace OwlcatTest.Player
{
    [DisallowMultipleComponent]
    public sealed class IsometricCameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(-8f, 11f, -8f);
        [SerializeField, Min(0f)] private float followSharpness = 10f;

        private void LateUpdate()
        {
            if (target == null) return;
            var desired = target.position + offset;
            var blend = 1f - Mathf.Exp(-followSharpness * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, desired, blend);
            transform.LookAt(target.position);
        }

        private void OnValidate() => followSharpness = Mathf.Max(0f, followSharpness);
    }
}
