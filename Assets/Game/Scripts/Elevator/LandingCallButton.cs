using UnityEngine;

namespace OwlcatTest.Elevator
{
    [DisallowMultipleComponent]
    public sealed class LandingCallButton : MonoBehaviour
    {
        [SerializeField] private ElevatorController elevator;
        [SerializeField, Min(0)] private int floorIndex;

        public ElevatorController Elevator => elevator;
        public int FloorIndex => floorIndex;

        public void Call() => elevator?.CallToFloor(floorIndex);
    }
}
