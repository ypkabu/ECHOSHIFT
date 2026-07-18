using System;
using System.Collections.Generic;
using EchoShift.Player;
using EchoShift.Reset;
using UnityEngine;

namespace EchoShift.Interaction
{
    [RequireComponent(typeof(Collider))]
    public sealed class PressurePlate : MonoBehaviour, IResettable, IDoorOpenSource
    {
        private readonly HashSet<LoopActor> _occupants = new HashSet<LoopActor>();
        private readonly List<LoopActor> _invalidOccupants = new List<LoopActor>(4);
        private readonly Collider[] _overlapBuffer = new Collider[16];
        private BoxCollider _boxCollider;

        public event Action<bool> PressedChanged;

        public bool RequestsDoorOpen => IsPressed;

        public bool IsPressed
        {
            get
            {
                PruneInvalidOccupants();
                return IsPressedWithoutPruning;
            }
        }

        public int OccupantCount
        {
            get
            {
                PruneInvalidOccupants();
                return _occupants.Count;
            }
        }

        private bool IsPressedWithoutPruning => _occupants.Count > 0;

        public void CaptureInitialState()
        {
            _occupants.Clear();
        }

        public void RestoreInitialState()
        {
            ClearOccupants();
        }

        public void RegisterActor(LoopActor actor)
        {
            if (actor == null)
            {
                return;
            }

            PruneInvalidOccupants();
            bool wasPressed = IsPressedWithoutPruning;
            _occupants.Add(actor);
            NotifyIfChanged(wasPressed);
        }

        public void UnregisterActor(LoopActor actor)
        {
            PruneInvalidOccupants();
            bool wasPressed = IsPressedWithoutPruning;
            _occupants.Remove(actor);
            NotifyIfChanged(wasPressed);
        }

        public void RefreshFromPhysics()
        {
            _boxCollider ??= GetComponent<BoxCollider>();
            if (_boxCollider == null)
            {
                return;
            }

            bool wasPressed = IsPressedWithoutPruning;
            _occupants.Clear();
            Vector3 scale = transform.lossyScale;
            Vector3 halfExtents = Vector3.Scale(
                _boxCollider.size,
                new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z))) * 0.5f;
            Vector3 center = transform.TransformPoint(_boxCollider.center);
            int count = Physics.OverlapBoxNonAlloc(
                center,
                halfExtents,
                _overlapBuffer,
                transform.rotation,
                ~0,
                QueryTriggerInteraction.Collide);
            for (int i = 0; i < count; i++)
            {
                LoopActor actor = _overlapBuffer[i].GetComponentInParent<LoopActor>();
                if (actor != null && actor.isActiveAndEnabled &&
                    actor.gameObject.activeInHierarchy)
                {
                    _occupants.Add(actor);
                }

                _overlapBuffer[i] = null;
            }

            NotifyIfChanged(wasPressed);
        }

        private void Update()
        {
            PruneInvalidOccupants();
        }

        private void OnTriggerEnter(Collider other)
        {
            RegisterActor(other.GetComponentInParent<LoopActor>());
        }

        private void OnTriggerExit(Collider other)
        {
            UnregisterActor(other.GetComponentInParent<LoopActor>());
        }

        private void OnDisable()
        {
            ClearOccupants();
        }

        private void NotifyIfChanged(bool wasPressed)
        {
            bool isPressed = IsPressedWithoutPruning;
            if (wasPressed != isPressed)
            {
                PressedChanged?.Invoke(isPressed);
            }
        }

        private void PruneInvalidOccupants()
        {
            if (_occupants.Count == 0)
            {
                return;
            }

            bool wasPressed = IsPressedWithoutPruning;
            _invalidOccupants.Clear();
            foreach (LoopActor actor in _occupants)
            {
                if (actor == null || !actor.isActiveAndEnabled || !actor.gameObject.activeInHierarchy)
                {
                    _invalidOccupants.Add(actor);
                }
            }

            for (int i = 0; i < _invalidOccupants.Count; i++)
            {
                _occupants.Remove(_invalidOccupants[i]);
            }

            _invalidOccupants.Clear();
            NotifyIfChanged(wasPressed);
        }

        private void ClearOccupants()
        {
            bool wasPressed = IsPressedWithoutPruning;
            _occupants.Clear();
            if (wasPressed)
            {
                PressedChanged?.Invoke(false);
            }
        }
    }
}
