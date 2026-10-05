using OwlcatTest.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace OwlcatTest.Dialogue
{
    [DisallowMultipleComponent]
    public sealed class DialogueRunner : MonoBehaviour
    {
        [SerializeField] private PlayerMotor player;
        [SerializeField] private DialogueView view;
        [SerializeField] private DialogueDefinition definition;

        private DialogueDefinition.Node current;
        private int lineIndex;
        private int choiceIndex;
        private bool selecting;
        private int openedFrame;
        private PlayerInputReader input;

        public bool IsRunning => current != null;

        private void Awake() => input = player != null ? player.GetComponent<PlayerInputReader>() : null;

        public void StartDialogue()
        {
            if (IsRunning || player == null || view == null || definition == null) return;
            var errors = definition.Validate();
            if (errors.Count > 0)
            {
                Debug.LogError($"Invalid dialogue {definition.name}: {string.Join("; ", errors)}", definition);
                return;
            }
            if (!player.TryEnterMode(PlayerMode.Dialogue)) return;
            definition.TryGetNode(definition.StartNodeId, out current);
            lineIndex = 0;
            choiceIndex = 0;
            selecting = false;
            openedFrame = Time.frameCount;
            view.SetInteractLabel(input != null ? input.InteractBindingLabel : "E");
            DisplayLine();
        }

        private void Update()
        {
            if (!IsRunning || Keyboard.current == null || Time.frameCount == openedFrame) return;
            var keyboard = Keyboard.current;
            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                EndDialogue();
                return;
            }
            if (selecting)
            {
                var count = current.choices.Count;
                if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame)
                    choiceIndex = (choiceIndex - 1 + count) % count;
                if (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame)
                    choiceIndex = (choiceIndex + 1) % count;
                view.ShowChoices(current.choices, choiceIndex);
                if (ConfirmPressed(keyboard)) FollowChoice();
                return;
            }
            if (!ConfirmPressed(keyboard)) return;
            lineIndex++;
            if (lineIndex < current.lines.Count)
            {
                DisplayLine();
                return;
            }
            if (current.choices == null || current.choices.Count == 0)
            {
                EndDialogue();
                return;
            }
            selecting = true;
            choiceIndex = 0;
            view.ShowChoices(current.choices, choiceIndex);
        }

        private bool ConfirmPressed(Keyboard keyboard) =>
            (input != null && input.InteractPressed) || keyboard.enterKey.wasPressedThisFrame ||
            keyboard.numpadEnterKey.wasPressedThisFrame;

        private void FollowChoice()
        {
            var nextId = current.choices[choiceIndex].nextNodeId;
            if (string.IsNullOrEmpty(nextId) || !definition.TryGetNode(nextId, out var next))
            {
                EndDialogue();
                return;
            }
            current = next;
            lineIndex = 0;
            selecting = false;
            DisplayLine();
        }

        private void DisplayLine()
        {
            view.ShowLine(current.speaker, current.lines[lineIndex]);
            view.ShowContinueHint();
        }

        private void EndDialogue()
        {
            current = null;
            selecting = false;
            view?.Hide();
            player?.ExitMode(PlayerMode.Dialogue);
        }

        private void OnDisable()
        {
            if (IsRunning) EndDialogue();
        }
    }
}
