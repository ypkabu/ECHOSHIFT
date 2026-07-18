using System;
using UnityEngine;

namespace EchoShift.Reset
{
    public sealed class ResetRegistry : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour[] resettableComponents = Array.Empty<MonoBehaviour>();

        private IResettable[] _resettables = Array.Empty<IResettable>();
        private bool _isReady;

        public int Count => _resettables.Length;

        public void Configure(MonoBehaviour[] components)
        {
            resettableComponents = components ?? Array.Empty<MonoBehaviour>();
        }

        private void Awake()
        {
            BuildCache();
        }

        public void CaptureInitialStates()
        {
            EnsureReady();
            for (int i = 0; i < _resettables.Length; i++)
            {
                _resettables[i].CaptureInitialState();
            }
        }

        public void RestoreInitialStates()
        {
            EnsureReady();
            for (int i = 0; i < _resettables.Length; i++)
            {
                _resettables[i].RestoreInitialState();
            }
        }

        private void BuildCache()
        {
            _resettables = new IResettable[resettableComponents.Length];
            for (int i = 0; i < resettableComponents.Length; i++)
            {
                MonoBehaviour component = resettableComponents[i];
                if (component is not IResettable resettable)
                {
                    string componentName = component == null ? "null" : component.GetType().Name;
                    throw new InvalidOperationException(
                        $"Reset registry entry {i} ({componentName}) does not implement IResettable.");
                }

                _resettables[i] = resettable;
            }

            _isReady = true;
        }

        private void EnsureReady()
        {
            if (!_isReady)
            {
                BuildCache();
            }
        }
    }
}
