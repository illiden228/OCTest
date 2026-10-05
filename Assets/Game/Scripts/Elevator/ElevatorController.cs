using System;
using System.Collections.Generic;
using OwlcatTest.Doors;
using OwlcatTest.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace OwlcatTest.Elevator
{
    public enum ElevatorDoorMode { None, Cabin, Landing, Both }

    [DisallowMultipleComponent]
    public sealed class ElevatorController : MonoBehaviour
    {
        [Serializable]
        public sealed class Floor
        {
            public string label;
            public Transform stop;
            public SlidingDoor landingDoor;
            public LandingCallButton callButton;
        }

        private enum TravelPhase { Idle, Closing, Moving, Opening }

        [SerializeField] private PlayerMotor player;
        [SerializeField] private Transform cabin;
        [SerializeField] private ElevatorView view;
        [SerializeField] private ElevatorDoorMode doorMode = ElevatorDoorMode.Both;
        [SerializeField] private SlidingDoor cabinDoor;
        [SerializeField] private List<Floor> floors = new();
        [SerializeField, Min(0.1f)] private float travelSpeed = 3f;
        [SerializeField] private int startFloor;

        private CharacterController characterController;
        private Transform originalPlayerParent;
        private TravelPhase phase;
        private bool selecting;
        private int currentFloor;
        private int selectedFloor;
        private int targetFloor;
        private int openedFrame;
        private bool transportingPlayer;
        private PlayerInputReader input;

        private bool UsesCabinDoor => doorMode is ElevatorDoorMode.Cabin or ElevatorDoorMode.Both;
        private bool UsesLandingDoors => doorMode is ElevatorDoorMode.Landing or ElevatorDoorMode.Both;
        public int CurrentFloor => currentFloor;
        public bool IsMoving => phase != TravelPhase.Idle;

        private void Start()
        {
            input = player != null ? player.GetComponent<PlayerInputReader>() : null;
            if (cabin == null || floors.Count == 0) return;
            currentFloor = Mathf.Clamp(startFloor, 0, floors.Count - 1);
            if (floors[currentFloor].stop != null) cabin.position = floors[currentFloor].stop.position;
            if (UsesCabinDoor) cabinDoor?.OpenForElevator();
            if (UsesLandingDoors)
                for (var i = 0; i < floors.Count; i++)
                {
                    if (i == currentFloor) floors[i].landingDoor?.OpenForElevator();
                    else floors[i].landingDoor?.CloseForElevator();
                }
        }

        public void OpenFloorSelector()
        {
            if (selecting || IsMoving || player == null || cabin == null || view == null || floors.Count == 0) return;
            var positionInCabin = cabin.InverseTransformPoint(player.transform.position);
            if (Mathf.Abs(positionInCabin.x) > 1.25f || Mathf.Abs(positionInCabin.z) > 1.25f ||
                positionInCabin.y < 0f || positionInCabin.y > 2.5f) return;
            if (!player.TryEnterMode(PlayerMode.Dialogue)) return;
            selecting = true;
            selectedFloor = currentFloor;
            openedFrame = Time.frameCount;
            view.SetInteractLabel(input != null ? input.InteractBindingLabel : "E");
            view.Show(floors, selectedFloor);
        }

        private void Update()
        {
            if (selecting) UpdateSelector();
            else if (phase != TravelPhase.Idle) UpdateTravel();
        }

        private void UpdateSelector()
        {
            if (Time.frameCount == openedFrame || Keyboard.current == null) return;
            var keyboard = Keyboard.current;
            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                CancelSelection();
                return;
            }
            if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame)
                selectedFloor = (selectedFloor - 1 + floors.Count) % floors.Count;
            if (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame)
                selectedFloor = (selectedFloor + 1) % floors.Count;
            view.Show(floors, selectedFloor);
            if (!(input != null && input.InteractPressed) && !keyboard.enterKey.wasPressedThisFrame &&
                !keyboard.numpadEnterKey.wasPressedThisFrame) return;
            if (selectedFloor == currentFloor)
            {
                CancelSelection();
                return;
            }
            BeginTravel(selectedFloor, true);
        }

        public void CallToFloor(int floor)
        {
            if (selecting || IsMoving || cabin == null || floor < 0 || floor >= floors.Count ||
                floor == currentFloor || floors[floor]?.stop == null) return;
            BeginTravel(floor, false);
        }

        private void BeginTravel(int floor, bool withPlayer)
        {
            if (floor < 0 || floor >= floors.Count || floors[floor].stop == null)
            {
                Debug.LogError("У целевого этажа лифта не задана точка остановки.", this);
                CancelSelection();
                return;
            }
            if (withPlayer)
            {
                if (player == null) return;
                selecting = false;
                view.Hide();
                player.ExitMode(PlayerMode.Dialogue);
                if (!player.TryEnterMode(PlayerMode.Traversal)) return;
            }
            transportingPlayer = withPlayer;
            targetFloor = floor;
            if (withPlayer)
            {
                characterController = player.GetComponent<CharacterController>();
                if (characterController != null) characterController.enabled = false;
                originalPlayerParent = player.transform.parent;
                player.transform.SetParent(cabin, true);
            }
            if (UsesCabinDoor) cabinDoor?.CloseForElevator();
            if (UsesLandingDoors) floors[currentFloor].landingDoor?.CloseForElevator();
            phase = TravelPhase.Closing;
        }

        private void UpdateTravel()
        {
            switch (phase)
            {
                case TravelPhase.Closing:
                    if ((!UsesCabinDoor || cabinDoor == null || cabinDoor.IsFullyClosed) &&
                        (!UsesLandingDoors || floors[currentFloor].landingDoor == null || floors[currentFloor].landingDoor.IsFullyClosed))
                        phase = TravelPhase.Moving;
                    break;
                case TravelPhase.Moving:
                    cabin.position = Vector3.MoveTowards(cabin.position, floors[targetFloor].stop.position,
                        travelSpeed * Time.deltaTime);
                    if ((cabin.position - floors[targetFloor].stop.position).sqrMagnitude > 0.0001f) break;
                    currentFloor = targetFloor;
                    if (UsesCabinDoor) cabinDoor?.OpenForElevator();
                    if (UsesLandingDoors) floors[currentFloor].landingDoor?.OpenForElevator();
                    phase = TravelPhase.Opening;
                    break;
                case TravelPhase.Opening:
                    if ((!UsesCabinDoor || cabinDoor == null || cabinDoor.IsFullyOpen) &&
                        (!UsesLandingDoors || floors[currentFloor].landingDoor == null || floors[currentFloor].landingDoor.IsFullyOpen))
                        FinishTravel();
                    break;
            }
        }

        private void FinishTravel()
        {
            if (transportingPlayer)
            {
                player.transform.SetParent(originalPlayerParent, true);
                if (characterController != null) characterController.enabled = true;
                player.ExitMode(PlayerMode.Traversal);
            }
            transportingPlayer = false;
            phase = TravelPhase.Idle;
        }

        private void CancelSelection()
        {
            selecting = false;
            view?.Hide();
            player?.ExitMode(PlayerMode.Dialogue);
        }

        private void OnDisable()
        {
            if (selecting) CancelSelection();
            if (IsMoving) FinishTravel();
        }

        private void OnValidate()
        {
            travelSpeed = Mathf.Max(0.1f, travelSpeed);
            startFloor = Mathf.Clamp(startFloor, 0, Mathf.Max(0, floors.Count - 1));
            if (floors.Count == 0) Debug.LogWarning("Лифту нужен хотя бы один этаж.", this);
            if (UsesCabinDoor && cabinDoor == null) Debug.LogWarning("Выбран режим с дверью кабины, но дверь не назначена.", this);
            if (UsesLandingDoors)
                foreach (var floor in floors)
                    if (floor != null && floor.landingDoor == null)
                        Debug.LogWarning($"У этажа {floor.label} не назначена дверь площадки.", this);
        }
    }
}
