using UnityEngine;

namespace OwlcatTest.Demo
{
    /// <summary>Visible consequence for the interaction prototype; replace with level logic later.</summary>
    public sealed class SignalLamp : MonoBehaviour
    {
        [SerializeField] private Renderer indicator;
        [SerializeField] private Light signalLight;
        [SerializeField] private Color offColor = Color.gray;
        [SerializeField] private Color onColor = Color.green;
        [SerializeField] private bool startsOn;

        private MaterialPropertyBlock properties;
        private bool isOn;

        private void Awake()
        {
            properties = new MaterialPropertyBlock();
            isOn = startsOn;
            Apply();
        }

        public void Toggle()
        {
            isOn = !isOn;
            Apply();
        }

        public void SetOn(bool value)
        {
            isOn = value;
            Apply();
        }

        private void Apply()
        {
            if (indicator != null)
            {
                indicator.GetPropertyBlock(properties);
                properties.SetColor("_BaseColor", isOn ? onColor : offColor);
                indicator.SetPropertyBlock(properties);
            }
            if (signalLight != null) signalLight.enabled = isOn;
        }
    }
}
