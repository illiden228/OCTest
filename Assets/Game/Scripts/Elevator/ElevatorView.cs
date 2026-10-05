using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace OwlcatTest.Elevator
{
    public sealed class ElevatorView : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Text choicesLabel;

        private readonly StringBuilder builder = new();
        private string interactLabel = "E";

        private void Awake() => Hide();

        public void SetInteractLabel(string label) => interactLabel = string.IsNullOrEmpty(label) ? "E" : label;

        public void Show(IReadOnlyList<ElevatorController.Floor> floors, int selected)
        {
            builder.Clear();
            builder.AppendLine("ВЫБЕРИТЕ ЭТАЖ");
            for (var i = 0; i < floors.Count; i++)
                builder.Append(i == selected ? "> " : "  ").AppendLine(floors[i].label);
            builder.Append("↑ ↓ — выбор   ").Append(interactLabel).AppendLine(" / Enter — ехать   Esc — закрыть");
            choicesLabel.text = builder.ToString();
            panel.SetActive(true);
        }

        public void Hide()
        {
            if (panel != null) panel.SetActive(false);
        }
    }
}
