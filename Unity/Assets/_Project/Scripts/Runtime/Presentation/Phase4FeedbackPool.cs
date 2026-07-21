using UnityEngine;

namespace EchoShift.Presentation
{
    public sealed class Phase4FeedbackPool : MonoBehaviour
    {
        [SerializeField] private GameObject effectPrefab;
        [SerializeField, Min(4)] private int capacity = 12;

        private ParticleSystem[] _systems = System.Array.Empty<ParticleSystem>();
        private int _next;

        public int Capacity => capacity;
        public int EmitCount { get; private set; }
        public bool HasValidReferences => effectPrefab != null && capacity >= 4;

        public void Configure(GameObject prefab, int poolCapacity)
        {
            effectPrefab = prefab;
            capacity = Mathf.Max(4, poolCapacity);
        }

        private void Awake()
        {
            if (!HasValidReferences)
            {
                Debug.LogError("Phase 4 feedback pool requires a prefab and capacity.", this);
                enabled = false;
                return;
            }

            _systems = new ParticleSystem[capacity];
            for (int i = 0; i < capacity; i++)
            {
                GameObject instance = Instantiate(effectPrefab, transform);
                instance.name = $"Feedback {i + 1:00}";
                ParticleSystem system = instance.GetComponent<ParticleSystem>();
                if (system == null)
                {
                    Debug.LogError("Phase 4 feedback prefab requires ParticleSystem.", instance);
                    enabled = false;
                    return;
                }
                system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                _systems[i] = system;
            }
        }

        public void Emit(Vector3 position, Color color, float size = 1f, float duration = 0.45f)
        {
            if (!enabled || _systems.Length == 0) return;
            ParticleSystem system = _systems[_next];
            _next = (_next + 1) % _systems.Length;
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            system.transform.position = position;
            ParticleSystem.MainModule main = system.main;
            main.startColor = color;
            main.startSize = Mathf.Clamp(size, 0.15f, 3f);
            main.startLifetime = Mathf.Clamp(duration, 0.12f, 1.2f);
            system.Play(true);
            EmitCount++;
        }
    }
}
