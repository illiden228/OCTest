using OwlcatTest.Interaction;
using UnityEditor;
using UnityEngine;

namespace OwlcatTest.Editor
{
    [CustomEditor(typeof(InteractionTarget))]
    public sealed class InteractionTargetEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var mode = serializedObject.FindProperty("activationMode");
            mode.enumValueIndex = EditorGUILayout.Popup("Способ активации", mode.enumValueIndex,
                new[] { "По нажатию кнопки", "При входе в вольюм" });
            EditorGUILayout.PropertyField(serializedObject.FindProperty("singleUse"), new GUIContent("Однократно"));

            if ((ActivationMode)mode.enumValueIndex == ActivationMode.Press)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Взаимодействие по кнопке", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("prompt"), new GUIContent("Подсказка"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("anchor"), new GUIContent("Точка взаимодействия"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("range"), new GUIContent("Радиус"));
                EditorGUILayout.HelpBox("Игрок нажимает кнопку действия в радиусе точки (E по умолчанию). Голубая сфера показывает зону действия.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Взаимодействие через вольюм", EditorStyles.boldLabel);
                var component = (InteractionTarget)target;
                var collider = component.GetComponent<Collider>();
                if (collider == null || !collider.isTrigger)
                    EditorGUILayout.HelpBox("Добавьте Collider с включённым Is Trigger на тот же объект.", MessageType.Error);
                else
                    EditorGUILayout.HelpBox("Вход в триггер вызывает событие без нажатия кнопки.", MessageType.Info);
            }

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onActivated"), new GUIContent("Событие при активации"));
            serializedObject.ApplyModifiedProperties();
        }
    }
}
