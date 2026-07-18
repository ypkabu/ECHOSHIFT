using EchoShift.Reset;
using UnityEngine;

namespace EchoShift.Interaction
{
    public sealed class DoorController : MonoBehaviour, IResettable
    {
        [SerializeField] private PressurePlate pressurePlate;
        [SerializeField] private Vector3 openOffset = new Vector3(0f, 3.5f, 0f);
        [SerializeField, Min(0.01f)] private float movementSpeed = 5f;

        private Vector3 _closedPosition;
        private bool _hasCapturedState;

        public bool HasValidReferences => pressurePlate != null;
        public bool IsOpen => _hasCapturedState &&
                              Vector3.SqrMagnitude(transform.position - _closedPosition) > 0.01f;

        public void Configure(PressurePlate plate, Vector3 offset, float speed)
        {
            pressurePlate = plate;
            openOffset = offset;
            movementSpeed = speed;
        }

        private void Awake()
        {
            if (pressurePlate == null)
            {
                Debug.LogError("DoorController requires a PressurePlate reference.", this);
                enabled = false;
            }
        }

        private void Update()
        {
            if (!_hasCapturedState)
            {
                return;
            }

            Vector3 target = pressurePlate.IsPressed
                ? _closedPosition + openOffset
                : _closedPosition;
            transform.position = Vector3.MoveTowards(
                transform.position,
                target,
                movementSpeed * Time.deltaTime);
        }

        public void CaptureInitialState()
        {
            _closedPosition = transform.position;
            _hasCapturedState = true;
        }

        public void RestoreInitialState()
        {
            if (_hasCapturedState)
            {
                transform.position = _closedPosition;
            }
        }
    }
}
