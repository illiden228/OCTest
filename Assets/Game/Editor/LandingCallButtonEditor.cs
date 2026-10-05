using OwlcatTest.Elevator;
using UnityEditor;
using UnityEngine;

namespace OwlcatTest.Editor
{
    [CustomEditor(typeof(LandingCallButton))]
    public sealed class LandingCallButtonEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.HelpBox("Эта кнопка вызывает пустую кабину на выбранный этаж. " +
                "Удобнее назначать её из карточки этажа на корневом объекте лифта.", MessageType.Info);
            var elevator = serializedObject.FindProperty("elevator");
            EditorGUILayout.PropertyField(elevator, new GUIContent("Лифт"));
            var index = serializedObject.FindProperty("floorIndex");
            if (elevator.objectReferenceValue != null)
            {
                var data = new SerializedObject(elevator.objectReferenceValue);
                var floors = data.FindProperty("floors");
                if (floors.arraySize > 0)
                {
                    var names = new string[floors.arraySize];
                    for (var i = 0; i < names.Length; i++)
                    {
                        var label = floors.GetArrayElementAtIndex(i).FindPropertyRelative("label").stringValue;
                        names[i] = string.IsNullOrWhiteSpace(label) ? "Этаж " + (i + 1) : label;
                    }
                    index.intValue = EditorGUILayout.Popup("Вызвать на этаж", Mathf.Clamp(index.intValue, 0, names.Length - 1), names);
                }
                else EditorGUILayout.HelpBox("В лифте ещё нет этажей.", MessageType.Warning);
                if (GUILayout.Button("Открыть настройки лифта"))
                {
                    Selection.activeObject = elevator.objectReferenceValue;
                    EditorGUIUtility.PingObject(elevator.objectReferenceValue);
                }
            }
            else EditorGUILayout.HelpBox("Назначьте корневой объект лифта.", MessageType.Error);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
