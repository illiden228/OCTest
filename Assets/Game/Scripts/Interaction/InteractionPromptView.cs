using UnityEngine;
using UnityEngine.UI;

namespace OwlcatTest.Interaction
{
    public sealed class InteractionPromptView : MonoBehaviour
    {
        [SerializeField] private Text label;
        [SerializeField] private bool showBindingInBottomLabel;
        private RectTransform marker;
        private Text markerGlyph;
        private RectTransform canvasRect;
        private Camera worldCamera;

        private void Awake()
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;
            canvasRect = canvas.transform as RectTransform;
            worldCamera = Camera.main;
            var icon = new GameObject("Focused interaction / E marker", typeof(RectTransform), typeof(Image), typeof(Outline));
            icon.transform.SetParent(canvas.transform, false);
            marker = icon.GetComponent<RectTransform>();
            marker.sizeDelta = new Vector2(62f, 62f);
            icon.GetComponent<Image>().color = new Color(0.04f, 0.08f, 0.11f, 0.94f);
            var border = icon.GetComponent<Outline>();
            border.effectColor = new Color(1f, 0.75f, 0.20f);
            border.effectDistance = new Vector2(3f, -3f);
            var glyph = new GameObject("E", typeof(RectTransform), typeof(Text));
            glyph.transform.SetParent(icon.transform, false);
            var rect = glyph.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var text = glyph.GetComponent<Text>();
            markerGlyph = text;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = "E";
            text.fontSize = 40;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(1f, 0.80f, 0.30f);
            text.raycastTarget = false;
            icon.GetComponent<Image>().raycastTarget = false;
            marker.gameObject.SetActive(false);
        }

        public void Show(string message) => Show(message, Vector3.zero, "E");

        public void Show(string message, Vector3 worldPosition, string bindingLabel)
        {
            if (label == null) return;
            var visible = !string.IsNullOrEmpty(message);
            if (visible) label.text = showBindingInBottomLabel ? bindingLabel + " — " + message : message;
            label.gameObject.SetActive(visible);
            if (markerGlyph != null) markerGlyph.text = bindingLabel;
            if (marker == null || canvasRect == null || worldCamera == null) return;
            var screen = visible ? worldCamera.WorldToScreenPoint(worldPosition) : Vector3.zero;
            visible &= screen.z > 0f && screen.x >= 0f && screen.x <= Screen.width &&
                       screen.y >= 0f && screen.y <= Screen.height;
            marker.gameObject.SetActive(visible);
            if (!visible) return;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen, null, out var point))
                marker.anchoredPosition = point;
            var pulse = 1f + 0.06f * Mathf.Sin(Time.unscaledTime * 5f);
            marker.localScale = Vector3.one * pulse;
        }
    }
}
