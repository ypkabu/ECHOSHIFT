using System;
using EchoShift.Reset;
using UnityEngine;

namespace EchoShift.Interaction
{
    public sealed class DoorController : MonoBehaviour, IResettable
    {
        [SerializeField] private MonoBehaviour[] openSourceComponents = Array.Empty<MonoBehaviour>();
        [SerializeField] private Vector3 openOffset = new Vector3(0f, 3.5f, 0f);
        [SerializeField, Min(0.01f)] private float movementSpeed = 5f;

        private IDoorOpenSource[] _openSources = Array.Empty<IDoorOpenSource>();
        private Vector3 _closedPosition;
        private bool _hasCapturedState;
        private bool _hasValidSources;
        private bool _usesCoordinatedTicks;
        private bool _committedOpenRequest;
        private bool _appliedOpenRequest;

        public bool HasValidReferences => _hasValidSources;
        public bool IsOpen => _hasCapturedState &&
                              Vector3.SqrMagnitude(transform.position - _closedPosition) > 0.01f;
        public bool IsOpenRequested
        {
            get
            {
                for (int i = 0; i < _openSources.Length; i++)
                {
                    if (_openSources[i].RequestsDoorOpen)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
        public bool AppliedOpenRequest => _appliedOpenRequest;

        public void Configure(PressurePlate plate, Vector3 offset, float speed)
        {
            Configure(new MonoBehaviour[] { plate }, offset, speed);
        }

        public void Configure(MonoBehaviour[] sources, Vector3 offset, float speed)
        {
            openSourceComponents = sources ?? Array.Empty<MonoBehaviour>();
            openOffset = offset;
            movementSpeed = speed;
            BuildSourceCache();
        }

        private void Awake()
        {
            if (!EnsureSourceCache())
            {
                Debug.LogError("DoorController requires at least one valid IDoorOpenSource.", this);
                enabled = false;
            }
        }

        public bool EnsureSourceCache()
        {
            BuildSourceCache();
            return _hasValidSources;
        }

        private void Update()
        {
            if (!_hasCapturedState || _usesCoordinatedTicks)
            {
                return;
            }

            Vector3 target = IsOpenRequested
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
            _committedOpenRequest = false;
            _appliedOpenRequest = false;
        }

        public void RestoreInitialState()
        {
            if (_hasCapturedState)
            {
                transform.position = _closedPosition;
            }

            _committedOpenRequest = false;
            _appliedOpenRequest = false;
        }

        public void UseCoordinatedTicks()
        {
            _usesCoordinatedTicks = true;
        }

        public void BeginSimulationTick()
        {
            if (!_usesCoordinatedTicks || !_hasCapturedState)
            {
                return;
            }

            _appliedOpenRequest = _committedOpenRequest;
            transform.position = _appliedOpenRequest
                ? _closedPosition + openOffset
                : _closedPosition;
        }

        public void CommitDeviceState()
        {
            if (_usesCoordinatedTicks)
            {
                _committedOpenRequest = IsOpenRequested;
            }
        }

        private void BuildSourceCache()
        {
            _openSources = new IDoorOpenSource[openSourceComponents.Length];
            _hasValidSources = openSourceComponents.Length > 0;
            for (int i = 0; i < openSourceComponents.Length; i++)
            {
                if (openSourceComponents[i] is not IDoorOpenSource source)
                {
                    _hasValidSources = false;
                    continue;
                }

                _openSources[i] = source;
            }
        }
    }
}
