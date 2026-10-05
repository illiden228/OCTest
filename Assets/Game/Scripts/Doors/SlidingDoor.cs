using System;
using System.Collections.Generic;
using OwlcatTest.Interaction;
using UnityEngine;

namespace OwlcatTest.Doors
{
    public enum DoorState { Closed, Open, Locked, Disabled }

    [DisallowMultipleComponent]
    public sealed class SlidingDoor : MonoBehaviour
    {
        [Serializable]
        public sealed class Leaf
        {
            public Transform transform;
            public Vector3 closedLocalPosition;
            public Vector3 openLocalPosition;
        }

        [SerializeField] private List<Leaf> leaves = new();
        [SerializeField] private DoorState initialState = DoorState.Closed;
        [SerializeField, Min(0.1f)] private float speed = 3f;
        [SerializeField] private Renderer stateIndicator;
        [SerializeField] private InteractionTarget interaction;

        private MaterialPropertyBlock properties;

        public DoorState State { get; private set; }
        public bool IsFullyClosed => AtTarget(false);
        public bool IsFullyOpen => AtTarget(true);

        private void Awake()
        {
            properties = new MaterialPropertyBlock();
            State = initialState;
            ApplyImmediate(State == DoorState.Open);
            UpdateIndicator();
        }

        private void Update()
        {
            var open = State == DoorState.Open;
            foreach (var leaf in leaves)
            {
                if (leaf?.transform == null) continue;
                var goal = open ? leaf.openLocalPosition : leaf.closedLocalPosition;
                leaf.transform.localPosition = Vector3.MoveTowards(leaf.transform.localPosition, goal, speed * Time.deltaTime);
            }
        }

        public void Toggle()
        {
            if (State == DoorState.Open) Close();
            else if (State == DoorState.Closed) Open();
        }

        public void Open()
        {
            if (State == DoorState.Closed) SetState(DoorState.Open);
        }

        public void Close()
        {
            if (State == DoorState.Open) SetState(DoorState.Closed);
        }

        public void Lock() => SetState(DoorState.Locked);
        public void Unlock()
        {
            if (State == DoorState.Locked) SetState(DoorState.Closed);
        }
        public void Disable() => SetState(DoorState.Disabled);
        public void Enable()
        {
            if (State == DoorState.Disabled) SetState(DoorState.Closed);
        }

        // Elevator orchestration is authoritative over its own doors.
        public void OpenForElevator() => SetState(DoorState.Open);
        public void CloseForElevator() => SetState(DoorState.Closed);

        private void SetState(DoorState value)
        {
            State = value;
            UpdateIndicator();
        }

        private bool AtTarget(bool open)
        {
            foreach (var leaf in leaves)
            {
                if (leaf?.transform == null) continue;
                var goal = open ? leaf.openLocalPosition : leaf.closedLocalPosition;
                if ((leaf.transform.localPosition - goal).sqrMagnitude > 0.0001f) return false;
            }
            return true;
        }

        private void ApplyImmediate(bool open)
        {
            foreach (var leaf in leaves)
                if (leaf?.transform != null)
                    leaf.transform.localPosition = open ? leaf.openLocalPosition : leaf.closedLocalPosition;
        }

        private void UpdateIndicator()
        {
            if (stateIndicator != null)
            {
                var color = State switch
                {
                    DoorState.Open => Color.green,
                    DoorState.Closed => Color.cyan,
                    DoorState.Locked => Color.red,
                    _ => Color.gray
                };
                stateIndicator.GetPropertyBlock(properties);
                properties.SetColor("_BaseColor", color);
                stateIndicator.SetPropertyBlock(properties);
            }
            if (interaction != null)
            {
                var label = State switch
                {
                    DoorState.Open => "Закрыть дверь",
                    DoorState.Closed => "Открыть дверь",
                    DoorState.Locked => "Дверь заперта",
                    _ => "Нет питания"
                };
                interaction.SetPrompt(label);
            }
        }

        private void OnValidate()
        {
            speed = Mathf.Max(0.1f, speed);
            if (leaves.Count is < 1 or > 3)
                Debug.LogWarning("У двери должно быть от одной до трёх створок.", this);
        }
    }
}
