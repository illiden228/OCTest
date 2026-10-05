using OwlcatTest.Doors;
using UnityEditor;
using UnityEngine;

namespace OwlcatTest.Editor
{
    [CustomEditor(typeof(SlidingDoor))]
    public sealed class SlidingDoorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.LabelField("Движение двери", EditorStyles.boldLabel);
            var state = serializedObject.FindProperty("initialState");
            state.enumValueIndex = EditorGUILayout.Popup("Начальное состояние", state.enumValueIndex,
                new[] { "Закрыта", "Открыта", "Заперта", "Выключена" });
            Draw("speed", "Скорость");
            var leaves = serializedObject.FindProperty("leaves");
            EditorGUILayout.PropertyField(leaves, new GUIContent("Створки (1–3)"), false);
            if (leaves.isExpanded)
            {
                leaves.arraySize = EditorGUILayout.IntSlider("Количество створок", leaves.arraySize, 1, 3);
                for (var i = 0; i < leaves.arraySize; i++)
                {
                    var leaf = leaves.GetArrayElementAtIndex(i);
                    EditorGUILayout.LabelField("Створка " + (i + 1), EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(leaf.FindPropertyRelative("transform"), new GUIContent("Объект створки"));
                    EditorGUILayout.PropertyField(leaf.FindPropertyRelative("closedLocalPosition"), new GUIContent("Позиция закрыта"));
                    EditorGUILayout.PropertyField(leaf.FindPropertyRelative("openLocalPosition"), new GUIContent("Позиция открыта"));
                }
            }
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Индикация и действие", EditorStyles.boldLabel);
            Draw("stateIndicator", "Индикатор состояния");
            Draw("interaction", "Интеракция");
            serializedObject.ApplyModifiedProperties();

            if (leaves.arraySize < 1 || leaves.arraySize > 3)
                EditorGUILayout.HelpBox("У двери должно быть от одной до трёх створок.", MessageType.Error);
            for (var i = 0; i < leaves.arraySize; i++)
            {
                var leaf = leaves.GetArrayElementAtIndex(i);
                if (leaf.FindPropertyRelative("transform").objectReferenceValue == null)
                    EditorGUILayout.HelpBox("У створки " + (i + 1) + " не назначен Transform.", MessageType.Error);
            }
            EditorGUILayout.HelpBox("Позиции створок задаются в локальных координатах их родителя. В Play Mode видны переходы между состояниями.", MessageType.Info);
        }

        private void Draw(string name, string label) => EditorGUILayout.PropertyField(serializedObject.FindProperty(name), new GUIContent(label));
    }
}
