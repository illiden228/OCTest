using OwlcatTest.Demo;
using UnityEditor;
using UnityEngine;

namespace OwlcatTest.Editor
{
    [CustomEditor(typeof(FloorVisibilityByHeight))]
    public sealed class FloorVisibilityByHeightEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("player"), new GUIContent("Игрок"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("mezzanineCover"),
                new GUIContent("Перекрытия второго этажа"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("showMezzanineAtHeight"),
                new GUIContent("Показать выше высоты"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("upperCover"),
                new GUIContent("Перекрытия верхнего этажа"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("showUpperAtHeight"),
                new GUIContent("Показать верхний этаж выше"));
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.HelpBox("Рендереры этажей скрываются, пока игрок находится ниже порога. Коллайдеры остаются активными.",
                MessageType.Info);
        }
    }
}
