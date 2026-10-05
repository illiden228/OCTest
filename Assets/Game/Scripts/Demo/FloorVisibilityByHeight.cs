using OwlcatTest.Player;
using UnityEngine;

namespace OwlcatTest.Demo
{
    [DisallowMultipleComponent]
    public sealed class FloorVisibilityByHeight : MonoBehaviour
    {
        [SerializeField] private PlayerMotor player;
        [SerializeField] private Renderer[] mezzanineCover;
        [SerializeField] private Renderer[] upperCover;
        [SerializeField] private float showMezzanineAtHeight = 3.2f;
        [SerializeField] private float showUpperAtHeight = 7.2f;

        private bool? mezzanineVisible;
        private bool? upperVisible;

        private void LateUpdate()
        {
            if (player == null) return;
            var height = player.transform.position.y;
            SetVisible(mezzanineCover, height >= showMezzanineAtHeight, ref mezzanineVisible);
            SetVisible(upperCover, height >= showUpperAtHeight, ref upperVisible);
        }

        private void OnDisable()
        {
            ShowAll(mezzanineCover);
            ShowAll(upperCover);
            mezzanineVisible = null;
            upperVisible = null;
        }

        private static void SetVisible(Renderer[] renderers, bool visible, ref bool? lastValue)
        {
            if (lastValue == visible) return;
            if (renderers != null)
                foreach (var item in renderers)
                    if (item != null) item.enabled = visible;
            lastValue = visible;
        }

        private static void ShowAll(Renderer[] renderers)
        {
            if (renderers == null) return;
            foreach (var item in renderers)
                if (item != null) item.enabled = true;
        }
    }
}
