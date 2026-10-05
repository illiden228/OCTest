using OwlcatTest.Elevator;
using UnityEditor;
using UnityEngine;

namespace OwlcatTest.Editor
{
    [CustomEditor(typeof(ElevatorController))]
    public sealed class ElevatorControllerEditor : UnityEditor.Editor
    {
        private static readonly string[] DoorModes =
            { "Без дверей", "Только кабина", "Только площадки", "Кабина и площадки" };

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var floors = serializedObject.FindProperty("floors");
            var mode = serializedObject.FindProperty("doorMode");
            EditorGUILayout.HelpBox("Весь маршрут лифта настраивается здесь. Точка остановки задаёт положение кабины; " +
                "дверь площадки и кнопка вызова указываются для каждого этажа отдельно.", MessageType.Info);

            EditorGUILayout.LabelField("Основные ссылки", EditorStyles.boldLabel);
            Reference(serializedObject.FindProperty("player"), "Игрок");
            Reference(serializedObject.FindProperty("cabin"), "Движущаяся кабина");
            Reference(serializedObject.FindProperty("view"), "Меню выбора этажа");
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Движение и двери", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("travelSpeed"), new GUIContent("Скорость кабины"));
            mode.enumValueIndex = EditorGUILayout.Popup("Какие двери использовать", mode.enumValueIndex, DoorModes);
            var doorMode = (ElevatorDoorMode)mode.enumValueIndex;
            if (doorMode == ElevatorDoorMode.Cabin || doorMode == ElevatorDoorMode.Both)
                Reference(serializedObject.FindProperty("cabinDoor"), "Дверь кабины");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Этажи — порядок совпадает с меню в игре", EditorStyles.boldLabel);
            for (var i = 0; i < floors.arraySize; i++)
            {
                var floor = floors.GetArrayElementAtIndex(i);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField(Title(floor, i), EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(floor.FindPropertyRelative("label"), new GUIContent("Название в меню"));
                Reference(floor.FindPropertyRelative("stop"), "Точка остановки");
                if (doorMode == ElevatorDoorMode.Landing || doorMode == ElevatorDoorMode.Both)
                    Reference(floor.FindPropertyRelative("landingDoor"), "Дверь площадки");
                Reference(floor.FindPropertyRelative("callButton"), "Кнопка вызова с площадки");
                var stop = floor.FindPropertyRelative("stop").objectReferenceValue as Transform;
                if (stop != null) EditorGUILayout.LabelField("Высота остановки", stop.position.y.ToString("0.##") + " м");
                EditorGUILayout.EndVertical();
            }

            var add = GUILayout.Button("Добавить этаж");
            if (add)
            {
                var index = floors.arraySize++;
                var floor = floors.GetArrayElementAtIndex(index);
                floor.FindPropertyRelative("label").stringValue = (index + 1) + " / Новый этаж";
                floor.FindPropertyRelative("stop").objectReferenceValue = null;
                floor.FindPropertyRelative("landingDoor").objectReferenceValue = null;
                floor.FindPropertyRelative("callButton").objectReferenceValue = null;
            }
            using (new EditorGUI.DisabledScope(floors.arraySize <= 1))
            {
                if (GUILayout.Button("Удалить последний этаж из маршрута"))
                {
                    var oldButton = floors.GetArrayElementAtIndex(floors.arraySize - 1)
                        .FindPropertyRelative("callButton").objectReferenceValue as LandingCallButton;
                    if (oldButton != null)
                    {
                        Undo.RecordObject(oldButton, "Отвязать кнопку удалённого этажа");
                        var buttonData = new SerializedObject(oldButton);
                        buttonData.FindProperty("elevator").objectReferenceValue = null;
                        buttonData.ApplyModifiedProperties();
                    }
                    floors.arraySize--;
                }
            }
            var start = serializedObject.FindProperty("startFloor");
            if (floors.arraySize > 0)
            {
                var names = new string[floors.arraySize];
                for (var i = 0; i < names.Length; i++) names[i] = Title(floors.GetArrayElementAtIndex(i), i);
                start.intValue = EditorGUILayout.Popup("Стартовый этаж", Mathf.Clamp(start.intValue, 0, names.Length - 1), names);
            }
            if (serializedObject.ApplyModifiedProperties()) SynchronizeButtons();
            if (GUILayout.Button("Проверить и связать кнопки вызова")) SynchronizeButtons();
            Validate(doorMode);
        }

        private void SynchronizeButtons()
        {
            var data = new SerializedObject(target);
            var floors = data.FindProperty("floors");
            for (var i = 0; i < floors.arraySize; i++)
            {
                var button = floors.GetArrayElementAtIndex(i).FindPropertyRelative("callButton")
                    .objectReferenceValue as LandingCallButton;
                if (button == null) continue;
                if (button.Elevator == target && button.FloorIndex == i) continue;
                Undo.RecordObject(button, "Настроить кнопку вызова лифта");
                var buttonData = new SerializedObject(button);
                buttonData.FindProperty("elevator").objectReferenceValue = target;
                buttonData.FindProperty("floorIndex").intValue = i;
                buttonData.ApplyModifiedProperties();
            }
        }

        private void Validate(ElevatorDoorMode doorMode)
        {
            var data = new SerializedObject(target);
            var floors = data.FindProperty("floors");
            if (data.FindProperty("player").objectReferenceValue == null ||
                data.FindProperty("cabin").objectReferenceValue == null ||
                data.FindProperty("view").objectReferenceValue == null)
                EditorGUILayout.HelpBox("Назначьте игрока, кабину и меню выбора этажа.", MessageType.Error);
            if (floors.arraySize == 0) EditorGUILayout.HelpBox("Добавьте хотя бы один этаж.", MessageType.Error);
            if ((doorMode == ElevatorDoorMode.Cabin || doorMode == ElevatorDoorMode.Both) &&
                data.FindProperty("cabinDoor").objectReferenceValue == null)
                EditorGUILayout.HelpBox("Для выбранного режима нужна дверь кабины.", MessageType.Error);
            for (var i = 0; i < floors.arraySize; i++)
            {
                var floor = floors.GetArrayElementAtIndex(i);
                if (floor.FindPropertyRelative("stop").objectReferenceValue == null)
                    EditorGUILayout.HelpBox("Этаж " + (i + 1) + ": не назначена точка остановки.", MessageType.Error);
                if ((doorMode == ElevatorDoorMode.Landing || doorMode == ElevatorDoorMode.Both) &&
                    floor.FindPropertyRelative("landingDoor").objectReferenceValue == null)
                    EditorGUILayout.HelpBox("Этаж " + (i + 1) + ": не назначена дверь площадки.", MessageType.Error);
                var button = floor.FindPropertyRelative("callButton").objectReferenceValue as LandingCallButton;
                if (button != null && (button.Elevator != target || button.FloorIndex != i))
                    EditorGUILayout.HelpBox("Этаж " + (i + 1) + ": кнопка указывает на другой этаж. Нажмите «Проверить и связать кнопки вызова».", MessageType.Warning);
            }
            EditorGUILayout.HelpBox("Передвигая этаж, переместите вместе точку остановки, площадку и дверь. " +
                "Для нового этажа создайте эти объекты и назначьте их в карточке.", MessageType.Info);
        }

        private static string Title(SerializedProperty floor, int index)
        {
            var label = floor.FindPropertyRelative("label").stringValue;
            return string.IsNullOrWhiteSpace(label) ? "Этаж " + (index + 1) : label;
        }

        private static void Reference(SerializedProperty property, string label)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(property, new GUIContent(label));
            using (new EditorGUI.DisabledScope(property.objectReferenceValue == null))
            {
                if (GUILayout.Button("Найти", GUILayout.Width(55)))
                {
                    Selection.activeObject = property.objectReferenceValue;
                    EditorGUIUtility.PingObject(property.objectReferenceValue);
                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
