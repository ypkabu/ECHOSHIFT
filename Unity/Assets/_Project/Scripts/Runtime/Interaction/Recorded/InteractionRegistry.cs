using System;
using System.Collections.Generic;
using UnityEngine;

namespace EchoShift.Interaction.Recorded
{
    public sealed class InteractionRegistry : MonoBehaviour
    {
        private readonly Dictionary<string, StableId> _targets =
            new Dictionary<string, StableId>(8, StringComparer.Ordinal);

        public int Count => _targets.Count;

        public bool Register(StableId stableId)
        {
            if (stableId == null ||
                string.IsNullOrWhiteSpace(stableId.Value) ||
                !stableId.CanRegister)
            {
                return false;
            }

            if (_targets.TryGetValue(stableId.Value, out StableId existing))
            {
                return existing == stableId;
            }

            _targets.Add(stableId.Value, stableId);
            return true;
        }

        public void Unregister(StableId stableId)
        {
            if (stableId == null || string.IsNullOrWhiteSpace(stableId.Value))
            {
                return;
            }

            if (_targets.TryGetValue(stableId.Value, out StableId existing) &&
                existing == stableId)
            {
                _targets.Remove(stableId.Value);
            }
        }

        public bool TryResolve(string stableId, out IInteractable target)
        {
            target = null;
            if (string.IsNullOrWhiteSpace(stableId) ||
                !_targets.TryGetValue(stableId, out StableId identity))
            {
                return false;
            }

            if (identity == null ||
                !identity.isActiveAndEnabled ||
                !identity.gameObject.activeInHierarchy ||
                !identity.IsTargetAvailable)
            {
                _targets.Remove(stableId);
                return false;
            }

            target = identity.Target;
            return true;
        }

        private void OnDisable()
        {
            _targets.Clear();
        }
    }
}
