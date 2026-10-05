using System;
using System.Collections.Generic;
using UnityEngine;

namespace OwlcatTest.Dialogue
{
    [CreateAssetMenu(menuName = "Сервисный пост/Диалог", fileName = "Dialogue")]
    public sealed class DialogueDefinition : ScriptableObject
    {
        [Serializable]
        public sealed class Choice
        {
            public string text;
            [Tooltip("Пустой ID завершает диалог.")]
            public string nextNodeId;
        }

        [Serializable]
        public sealed class Node
        {
            public string id;
            public string speaker;
            [TextArea(2, 5)] public List<string> lines = new();
            public List<Choice> choices = new();
        }

        [SerializeField] private string startNodeId = "start";
        [SerializeField] private List<Node> nodes = new();

        public string StartNodeId => startNodeId;
        public IReadOnlyList<Node> Nodes => nodes;

        public bool TryGetNode(string id, out Node node)
        {
            node = null;
            if (string.IsNullOrWhiteSpace(id)) return false;
            foreach (var candidate in nodes)
            {
                if (candidate != null && candidate.id == id)
                {
                    node = candidate;
                    return true;
                }
            }
            return false;
        }

        public List<string> Validate()
        {
            var errors = new List<string>();
            var ids = new HashSet<string>();
            foreach (var node in nodes)
            {
                if (node == null || string.IsNullOrWhiteSpace(node.id))
                {
                    errors.Add("У узла нет ID.");
                    continue;
                }
                if (!ids.Add(node.id)) errors.Add($"Повторяется ID узла: {node.id}");
                if (node.lines == null || node.lines.Count == 0)
                    errors.Add($"У узла {node.id} нет реплик NPC.");
            }
            if (!ids.Contains(startNodeId)) errors.Add($"Не найден начальный узел: {startNodeId}");
            foreach (var node in nodes)
            {
                if (node?.choices == null) continue;
                foreach (var choice in node.choices)
                {
                    if (choice == null || string.IsNullOrWhiteSpace(choice.text))
                        errors.Add($"У узла {node.id} есть пустой ответ.");
                    if (choice != null && !string.IsNullOrEmpty(choice.nextNodeId) && !ids.Contains(choice.nextNodeId))
                        errors.Add($"Узел {node.id} ссылается на отсутствующий узел {choice.nextNodeId}.");
                }
            }
            return errors;
        }
    }
}
