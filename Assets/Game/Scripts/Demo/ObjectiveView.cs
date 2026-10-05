using UnityEngine;
using UnityEngine.UI;

namespace OwlcatTest.Demo
{
    public sealed class ObjectiveView : MonoBehaviour
    {
        [SerializeField] private Text label;

        public void SetObjective(string value)
        {
            if (label != null) label.text = value;
        }
    }
}
