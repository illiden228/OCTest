using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace OwlcatTest.Dialogue
{
    public sealed class DialogueView : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Text speakerLabel;
        [SerializeField] private Text lineLabel;
        [SerializeField] private Text choicesLabel;

        private readonly StringBuilder builder = new();
        private string interactLabel = "E";

        private void Awake() => Hide();

        public void SetInteractLabel(string label) => interactLabel = string.IsNullOrEmpty(label) ? "E" : label;

        public void ShowLine(string speaker, string line)
        {
            panel.SetActive(true);
            speakerLabel.text = speaker;
            lineLabel.text = line;
            ShowContinueHint();
        }

        public void ShowChoices(IReadOnlyList<DialogueDefinition.Choice> choices, int selected)
        {
            builder.Clear();
            for (var i = 0; i < choices.Count; i++)
            {
                if (i > 0) builder.AppendLine();
                builder.Append(i == selected ? "> " : "  ").Append(choices[i].text);
            }
            builder.AppendLine().Append("↑ ↓ — выбор     ").Append(interactLabel).Append(" / Enter — подтвердить");
            choicesLabel.text = builder.ToString();
        }

        public void ShowContinueHint() => choicesLabel.text = interactLabel + " / Enter — дальше";

        public void Hide()
        {
            if (panel != null) panel.SetActive(false);
        }
    }
}
