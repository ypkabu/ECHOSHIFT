using System;
using EchoShift.Interaction;
using UnityEngine;

namespace EchoShift.Presentation
{
    /// <summary>
    /// Mirrors the existing door request state into the approved Phase 5A goal presentation.
    /// This adapter never writes gameplay state or moves gameplay transforms.
    /// </summary>
    public sealed class Phase5AGoalIdentityVisual : MonoBehaviour
    {
        [SerializeField] private DoorController[] requiredDoors = Array.Empty<DoorController>();
        [SerializeField] private GameObject lockedVisual;
        [SerializeField] private GameObject unlockedVisual;

        private bool _lastUnlocked;

        public bool HasRequiredReferences =>
            requiredDoors != null && requiredDoors.Length > 0 &&
            lockedVisual != null && unlockedVisual != null;
        public bool IsUnlockedVisual => _lastUnlocked;

        public void Configure(DoorController[] doors, GameObject locked, GameObject unlocked)
        {
            requiredDoors = doors ?? Array.Empty<DoorController>();
            lockedVisual = locked;
            unlockedVisual = unlocked;
            RefreshNowForTests();
        }

        private void OnEnable() => RefreshNowForTests();

        private void LateUpdate() => Apply(AllDoorsRequestOpen());

        public void RefreshNowForTests() => Apply(AllDoorsRequestOpen());

        public void SetEvidenceStateForEditor(bool unlocked) => Apply(unlocked);

        private bool AllDoorsRequestOpen()
        {
            if (requiredDoors == null || requiredDoors.Length == 0) return false;
            for (int i = 0; i < requiredDoors.Length; i++)
            {
                if (requiredDoors[i] == null || !requiredDoors[i].IsOpenRequested) return false;
            }
            return true;
        }

        private void Apply(bool unlocked)
        {
            _lastUnlocked = unlocked;
            if (lockedVisual != null) lockedVisual.SetActive(!unlocked);
            if (unlockedVisual != null) unlockedVisual.SetActive(unlocked);
        }
    }
}
