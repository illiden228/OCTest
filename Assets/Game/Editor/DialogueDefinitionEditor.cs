using OwlcatTest.Dialogue;
using UnityEditor;
using UnityEngine;

namespace OwlcatTest.Editor
{
    [CustomEditor(typeof(DialogueDefinition))]
    public sealed class DialogueDefinitionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("startNodeId"),
                new GUIContent("ID начального узла"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("nodes"),
                new GUIContent("Узлы и ответы"), true);
            serializedObject.ApplyModifiedProperties();

            var definition = (DialogueDefinition)target;
            var errors = definition.Validate();
            if (errors.Count == 0)
            {
                EditorGUILayout.HelpBox("Диалог настроен без ошибок.", MessageType.Info);
                return;
            }
            foreach (var error in errors)
                EditorGUILayout.HelpBox(error, MessageType.Error);
        }
    }
}
