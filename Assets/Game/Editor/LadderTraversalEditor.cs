using OwlcatTest.Traversal;
using UnityEditor;
using UnityEngine;

namespace OwlcatTest.Editor
{
    [CustomEditor(typeof(LadderTraversal))]
    public sealed class LadderTraversalEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.LabelField("Ссылки сцены", EditorStyles.boldLabel);
            Draw("player", "Игрок");
            Draw("bottomPoint", "Нижняя точка");
            Draw("topPoint", "Верхняя точка");
            Draw("visual", "Визуал лестницы");
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Перемещение", EditorStyles.boldLabel);
            Draw("height", "Высота");
            Draw("ascentSpeed", "Скорость подъёма");
            Draw("descentSpeed", "Скорость спуска");
            Draw("sprintMultiplier", "Множитель спринта");
            serializedObject.ApplyModifiedProperties();

            var bottom = serializedObject.FindProperty("bottomPoint").objectReferenceValue as Transform;
            var top = serializedObject.FindProperty("topPoint").objectReferenceValue as Transform;
            if (bottom != null && top != null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Точки перемещения", EditorStyles.boldLabel);
                EditorGUI.BeginChangeCheck();
                var bottomPosition = EditorGUILayout.Vector3Field("Нижняя точка (локально)", bottom.localPosition);
                var topXZ = EditorGUILayout.Vector2Field("Верхняя точка X/Z",
                    new Vector2(top.localPosition.x, top.localPosition.z));
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObjects(new Object[] { bottom, top }, "Настроить точки лестницы");
                    bottom.localPosition = bottomPosition;
                    top.localPosition = new Vector3(topXZ.x,
                        bottomPosition.y + serializedObject.FindProperty("height").floatValue, topXZ.y);
                    EditorUtility.SetDirty(bottom);
                    EditorUtility.SetDirty(top);
                }
            }

            if (serializedObject.FindProperty("player").objectReferenceValue == null ||
                serializedObject.FindProperty("bottomPoint").objectReferenceValue == null ||
                serializedObject.FindProperty("topPoint").objectReferenceValue == null)
                EditorGUILayout.HelpBox("Назначьте игрока, нижнюю и верхнюю точки.", MessageType.Error);
            else
                EditorGUILayout.HelpBox("Высота задаёт Y верхней точки; X/Z настраиваются отдельно. Жёлтые gizmo показывают путь.", MessageType.Info);
        }

        private void Draw(string name, string label) => EditorGUILayout.PropertyField(serializedObject.FindProperty(name), new GUIContent(label));
    }
}
